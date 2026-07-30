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
    private readonly IEmailService _emailService;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IEmailService emailService = null)
    {
        _authService = authService;
        _userRepository = userRepository;
        _emailService = emailService;
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return NotFound("User not found");

        var resetToken = $"RST-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        if (_emailService != null)
        {
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL NOTICE] SendPasswordResetEmailAsync failed: {ex.Message}");
            }
        }

        return Ok(new { Message = "Password reset instructions have been sent to your email.", ResetToken = resetToken });
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

        if (_emailService != null)
        {
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, "Password reset completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL NOTICE] SendPasswordResetEmailAsync failed: {ex.Message}");
            }
        }

        return Ok("Password reset successfully.");
    }
}