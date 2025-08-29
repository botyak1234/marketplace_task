using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Service.Abstractions;
using TaskMarketplace.Contracts.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.WebApi.Controllers;

/// <summary>
/// Контроллер для работы с данными текущего пользователя
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;
    public MeController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Получение количества баллов текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Количество баллов пользователя</returns>
    /// <response code="200">Успешное получение баллов</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="404">Пользователь не найден</response>
    [HttpGet("points")]
    [ProducesResponseType(typeof(PointsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPoints(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var points = await _userService.GetPointsAsync(userId, cancellationToken);
        if (points is null) return NotFound("User not found");
        return Ok(new { points });
    }
}