using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.Service.Abstractions;
using TaskMarketplace.Contracts.Enums;

namespace TaskMarketplace.WebApi.Controllers;

/// <summary>
/// Контроллер для управления задачами
/// </summary>
/// <response code="400">Ошибка АПИ</response>
/// <response code="401">Неавторизованный пользователь</response>
/// <response code="403">Доступ запрещен</response>
/// <response code="404">Данные не найдены</response>
/// <response code="500">Ошибка сервера</response>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Получение списка всех задач
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список задач</returns>
    /// <remarks>
    /// Для обычных пользователей возвращаются только их собственные задачи или свободные задачи.
    /// Администраторы видят все задачи.
    /// </remarks>
    /// <response code="200">Успешное получение списка задач</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var list = await _taskService.GetAllAsync(userId, role, cancellationToken);
        return Ok(list);
    }

    /// <summary>
    /// Получение задачи по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Задача с указанным идентификатором</returns>
    /// <response code="200">Успешное получение задачи</response>
    /// <response code="404">Задача не найдена</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>
    /// Создание новой задачи (только для администраторов)
    /// </summary>
    /// <param name="request">Данные для создания задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданная задача</returns>
    /// <response code="201">Задача успешно создана</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновление задачи (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="request">Данные для обновления задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции обновления</returns>
    /// <response code="204">Задача успешно обновлена</response>
    /// <response code="404">Задача не найдена</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var ok = await _taskService.UpdateAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Удаление задачи (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции удаления</returns>
    /// <response code="204">Задача успешно удалена</response>
    /// <response code="404">Задача не найдена</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ok = await _taskService.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Взятие задачи в работу
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная задачи</returns>
    /// <response code="200">Задача успешно взята в работу</response>
    /// <response code="400">Невозможно взять задачу (уже занята или неверный статус)</response>
    [HttpPost("{id:int}/take")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Take(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.TakeAsync(id, userId, cancellationToken);
        return updated is null ? BadRequest("Невозможно взять задачу") : Ok(updated);
    }

    /// <summary>
    /// Отправка задачи на проверку
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная задача</returns>
    /// <response code="200">Задача успешно отправлена на проверку</response>
    /// <response code="400">Невозможно отправить задачу (не взята пользователем или неверный статус)</response>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(int id, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.SubmitAsync(id, userId, cancellationToken);
        if (updated is null) return BadRequest("Задача должна быть взята вами и иметь статус 'В работе'");
        return Ok(updated);
    }

    /// <summary>
    /// Проверка задачи администратором (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="request">Решение по задаче</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная задача</returns>
    /// <response code="200">Задача успешно проверена</response>
    /// <response code="400">Неверный статус проверки</response>
    [HttpPost("{id:int}/review")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewRequest request, CancellationToken cancellationToken)
    {
        var updated = await _taskService.ReviewAsync(id, request.StatusByAdmin, cancellationToken);
        if (updated is null) return BadRequest("Неверный статус. Допустимые значения: Approved, Rejected");
        return Ok(updated);
    }

    /// <summary>
    /// Получение задач по статусу
    /// </summary>
    /// <param name="status">Статус задачи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список задач с указанным статусом</returns>
    /// <response code="200">Успешное получение задач</response>
    /// <response code="400">Неверный статус</response>
    [HttpGet("by-status")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByStatus([FromQuery] string status, CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetByStatusAsync(status, cancellationToken);
        if (tasks is null)
            return BadRequest($"Неверный статус. Допустимые значения: {string.Join(", ", Enum.GetNames(typeof(MarketplaceTaskStatus)))}");

        return Ok(tasks);
    }

    /// <summary>
    /// Получение задач с сортировкой
    /// </summary>
    /// <param name="sortBy">Поле для сортировки (created, updated)</param>
    /// <param name="order">Порядок сортировки (asc, desc)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Отсортированный список задач</returns>
    /// <response code="200">Успешное получение задач</response>
    [HttpGet("sorted")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSorted([FromQuery] string? sortBy, [FromQuery] string? order, CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetSortedAsync(sortBy, order, cancellationToken);
        return Ok(tasks);
    }
}