using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    private readonly IUserService _userService;
    public MeController(IUserService userService) => _userService = userService;

    [HttpGet("points")]
    public async Task<IActionResult> GetPoints()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var points = await _userService.GetPointsAsync(userId);
        if (points is null) return NotFound("User not found");
        return Ok(new { points });
    }
}
