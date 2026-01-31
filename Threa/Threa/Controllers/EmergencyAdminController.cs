using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Threa.Dal;

namespace Threa.Controllers;

/// <summary>
/// Emergency admin endpoint for password reset when locked out.
/// Access via: /api/emergency/reset-password?key=YOUR_SECRET&email=user@example.com&password=newpassword
///
/// SECURITY: This endpoint should be removed or disabled after use.
/// Set EMERGENCY_ADMIN_KEY environment variable to enable.
/// </summary>
[ApiController]
[Route("api/emergency")]
[AllowAnonymous]
public class EmergencyAdminController : ControllerBase
{
    private readonly IPlayerDal _playerDal;
    private readonly IConfiguration _configuration;

    public EmergencyAdminController(IPlayerDal playerDal, IConfiguration configuration)
    {
        _playerDal = playerDal;
        _configuration = configuration;
    }

    [HttpGet("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromQuery] string key,
        [FromQuery] string email,
        [FromQuery] string password)
    {
        // Check if emergency access is enabled
        var adminKey = _configuration["EMERGENCY_ADMIN_KEY"]
            ?? Environment.GetEnvironmentVariable("EMERGENCY_ADMIN_KEY");

        if (string.IsNullOrEmpty(adminKey))
        {
            return NotFound(); // Pretend endpoint doesn't exist
        }

        if (key != adminKey)
        {
            return Unauthorized("Invalid key");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email required");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return BadRequest("Password required (min 6 chars)");
        }

        var player = await _playerDal.GetPlayerByEmailAsync(email);
        if (player == null)
        {
            return NotFound($"User not found: {email}");
        }

        // Reset password
        player.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(password, player.Salt);

        // Clear any lockouts
        player.FailedRecoveryAttempts = 0;
        player.RecoveryLockoutUntil = null;

        await _playerDal.SavePlayerAsync(player);

        return Ok($"Password reset for {email}");
    }

    [HttpGet("list-users")]
    public async Task<IActionResult> ListUsers([FromQuery] string key)
    {
        var adminKey = _configuration["EMERGENCY_ADMIN_KEY"]
            ?? Environment.GetEnvironmentVariable("EMERGENCY_ADMIN_KEY");

        if (string.IsNullOrEmpty(adminKey))
        {
            return NotFound();
        }

        if (key != adminKey)
        {
            return Unauthorized("Invalid key");
        }

        var players = await _playerDal.GetAllPlayersAsync();
        var result = players.Select(p => new
        {
            p.Email,
            p.Name,
            p.IsEnabled,
            p.Roles,
            IsLockedOut = p.RecoveryLockoutUntil > DateTime.UtcNow
        });

        return Ok(result);
    }

    [HttpGet("grant-role")]
    public async Task<IActionResult> GrantRole(
        [FromQuery] string key,
        [FromQuery] string email,
        [FromQuery] string role)
    {
        var adminKey = _configuration["EMERGENCY_ADMIN_KEY"]
            ?? Environment.GetEnvironmentVariable("EMERGENCY_ADMIN_KEY");

        if (string.IsNullOrEmpty(adminKey))
        {
            return NotFound();
        }

        if (key != adminKey)
        {
            return Unauthorized("Invalid key");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email required");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return BadRequest("Role required (Administrator, GameMaster, Player)");
        }

        var player = await _playerDal.GetPlayerByEmailAsync(email);
        if (player == null)
        {
            return NotFound($"User not found: {email}");
        }

        // Parse existing roles
        var roles = string.IsNullOrEmpty(player.Roles)
            ? new List<string>()
            : player.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

        // Add role if not present
        if (!roles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            roles.Add(role);
            player.Roles = string.Join(",", roles);
            await _playerDal.SavePlayerAsync(player);
        }

        return Ok($"Granted {role} to {email}. Current roles: {player.Roles}");
    }
}
