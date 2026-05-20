using BGVSystem.Application.DTOs;
//using BGVSystem.Application.DTOs.Auth;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class AuthService : IAuthService
{
    public async Task<string> RegisterAsync(RegisterRequestDto dto)
    {
        await Task.CompletedTask;

        return "User registered successfully";
    }

    public async Task<string> LoginAsync(LoginRequestDto dto)
    {
        await Task.CompletedTask;

        return "Login successful";
    }
}