using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Contracts.Auth;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(result.ErrorMessage);
        return Ok("Registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _userService.LoginAsync(request);
        if (token is null) return Unauthorized();
        return Ok(new { token });
    }
}
