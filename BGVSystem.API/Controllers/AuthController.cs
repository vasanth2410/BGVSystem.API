using BGVSystem.Application.DTOs.Auth;
using BGVSystem.Application.Interfaces;
using BGVSystem.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    private readonly IUserRepository _userRepository;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    string email,
    string newPassword = "Debug@123")
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return NotFound("User not found");

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(newPassword);

        await _userRepository.SaveChangesAsync();

        return Ok("Password reset successfully.");
    }
}