using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.WebApi.Controllers;

/// <summary>
/// Контроллер для аутентификации и регистрации пользователей
/// </summary>
/// <response code="400">Ошибка АПИ</response>
/// <response code="401">Неавторизованный пользователь</response>
/// <response code="403">Доступ запрещен</response>
/// <response code="404">Данные не найдены</response>
/// <response code="500">Ошибка сервера</response>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат регистрации</returns>
    /// <response code="204">Пользователь успешно зарегистрирован</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.RegisterAsync(request, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ErrorMessage);
            
        return NoContent();
    }

    /// <summary>
    /// Аутентификация пользователя
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>JWT токен</returns>
    /// <response code="200">Успешная аутентификация, возвращает JWT токен</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await _userService.LoginAsync(request, cancellationToken);
        
        if (token == null)
            return Unauthorized("Неверные учетные данные");
            
        return Ok(token);
    }
}