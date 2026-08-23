using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Auth;

namespace Portfolio.Infrastructure.Identity;

/// <summary>
/// Verifies credentials via ASP.NET Core Identity's UserManager (which
/// handles password hashing/verification, lockout, etc.) and issues a
/// JWT on success. JWT rather than cookie auth - matches JWT
/// experience already listed on the site owner's own resume/prior
/// projects, and keeps the API stateless (no server-side session
/// store), consistent with it being a pure JSON API behind a separate
/// SPA rather than a server-rendered app.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            // Username only, never the password - logging credentials
            // (even failed ones) is exactly the "exposing sensitive
            // data" the spec warns against.
            _logger.LogWarning("Failed login attempt for unknown username {Username}", request.Username);
            return new LoginResult { Success = false };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
            return new LoginResult { Success = false };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
        {
            // Fail loudly rather than silently issuing an unsigned or
            // weakly-signed token - a missing signing key is a
            // deployment/configuration bug, not something to work
            // around at runtime.
            throw new InvalidOperationException(
                "Jwt:Key is not configured. Set it via 'dotnet user-secrets set \"Jwt:Key\" \"<value>\"'.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Id),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new LoginResult
        {
            Success = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expires,
            DisplayName = user.DisplayName,
        };
    }
}
