using BGVSystem.Application.DTOs.Auth;

namespace BGVSystem.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequestDto dto);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
}