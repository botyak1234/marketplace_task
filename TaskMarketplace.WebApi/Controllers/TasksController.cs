using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.Service.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.WebApi.Controllers;

/// <summary>
/// Контроллер для управления задачами
/// </summary>
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
    /// <returns>Список задач</returns>
    /// <remarks>
    /// Для обычных пользователей возвращаются только их собственные задачи или свободные задачи.
    /// Администраторы видят все задачи.
    /// </remarks>
    /// <response code="200">Успешное получение списка задач</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var list = await _taskService.GetAllAsync(userId, role);
        return Ok(list);
    }

    /// <summary>
    /// Получение задачи по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Задача с указанным идентификатором</returns>
    /// <response code="200">Успешное получение задачи</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="404">Задача не найдена</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>
    /// Создание новой задачи (только для администраторов)
    /// </summary>
    /// <param name="request">Данные для создания задачи</param>
    /// <returns>Созданная задача</returns>
    /// <response code="201">Задача успешно создана</response>
    /// <response code="400">Неверные данные для создания задачи</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав для создания задачи</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var created = await _taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновление задачи (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="request">Данные для обновления задачи</param>
    /// <returns>Результат операции обновления</returns>
    /// <response code="200">Задача успешно обновлена</response>
    /// <response code="400">Неверные данные для обновления задачи</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав для обновления задачи</response>
    /// <response code="404">Задача не найдена</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var ok = await _taskService.UpdateAsync(id, request);
        return ok ? Ok() : NotFound();
    }

    /// <summary>
    /// Удаление задачи (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Результат операции удаления</returns>
    /// <response code="204">Задача успешно удалена</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав для удаления задачи</response>
    /// <response code="404">Задача не найдена</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _taskService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Взятие задачи в работу
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Обновленная задача</returns>
    /// <response code="200">Задача успешно взята в работу</response>
    /// <response code="400">Невозможно взять задачу (уже занята или неверный статус)</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="404">Задача не найдена</response>
    [HttpPost("{id:int}/take")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Take(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.TakeAsync(id, userId);
        return updated is null ? BadRequest("Cannot take task") : Ok(updated);
    }

    /// <summary>
    /// Отправка задачи на проверку
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <returns>Обновленная задача</returns>
    /// <response code="200">Задача успешно отправлена на проверку</response>
    /// <response code="400">Невозможно отправить задачу (не взята пользователем или неверный статус)</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="404">Задача не найдена</response>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.SubmitAsync(id, userId);
        if (updated is null) return BadRequest("Task must be taken by you and be in 'Taken' status");
        return Ok(updated);
    }

    /// <summary>
    /// Проверка задачи администратором (только для администраторов)
    /// </summary>
    /// <param name="id">Идентификатор задачи</param>
    /// <param name="request">Решение по задаче</param>
    /// <returns>Обновленная задача</returns>
    /// <response code="200">Задача успешно проверена</response>
    /// <response code="400">Неверный статус проверки</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав для проверки задачи</response>
    /// <response code="404">Задача не найдена</response>
    [HttpPost("{id:int}/review")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewRequest request)
    {
        var updated = await _taskService.ReviewAsync(id, request.StatusByAdmin);
        if (updated is null) return BadRequest("Invalid status. Allowed values: Approved, Rejected");
        return Ok(updated);
    }
}