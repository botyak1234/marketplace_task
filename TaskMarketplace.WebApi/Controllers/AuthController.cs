using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.Service.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace TaskMarketplace.WebApi.Controllers;

/// <summary>
/// Контроллер для аутентификации и регистрации пользователей
/// </summary>
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
    /// <returns>Результат операции регистрации</returns>
    /// <response code="200">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Неверные данные для регистрации</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(result.ErrorMessage);
        return Ok("Registered");
    }

    /// <summary>
    /// Аутентификация пользователя
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <returns>JWT токен для аутентифицированного пользователя</returns>
    /// <response code="200">Успешная аутентификация, возвращает JWT токен</response>
    /// <response code="401">Неверные учетные данные</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _userService.LoginAsync(request);
        if (token is null) return Unauthorized();
        return Ok(new { token });
    }
}