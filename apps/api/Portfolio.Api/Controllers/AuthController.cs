using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Auth;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Success)
        {
            // Deliberately the same generic message whether the
            // username doesn't exist or the password is wrong - never
            // reveal which one, since that lets an attacker enumerate
            // valid usernames.
            return Unauthorized(new { error = "Invalid username or password." });
        }

        return Ok(new
        {
            token = result.Token,
            expiresAtUtc = result.ExpiresAtUtc,
            displayName = result.DisplayName,
        });
    }
}
