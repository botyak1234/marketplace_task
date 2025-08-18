using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Tasks;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.WebApi.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var list = await _taskService.GetAllAsync(userId, role);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var created = await _taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var ok = await _taskService.UpdateAsync(id, request);
        return ok ? Ok() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _taskService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/take")]
    public async Task<IActionResult> Take(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.TakeAsync(id, userId);
        return updated is null ? BadRequest("Cannot take task") : Ok(updated);
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await _taskService.SubmitAsync(id, userId);
        if (updated is null) return BadRequest("Task must be taken by you and be in 'Taken' status");
        return Ok(updated);
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewRequest request)
    {
        var updated = await _taskService.ReviewAsync(id, request.StatusByAdmin);
        if (updated is null) return BadRequest("Invalid status. Allowed values: Approved, Rejected");
        return Ok(updated);
    }
}
