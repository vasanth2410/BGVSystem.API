using BGVSystem.Application.DTOs.Auth;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    private readonly IJwtService _jwtService;

    public AuthService(
     IUserRepository userRepository,
     IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto dto)
    {
        var existingUser = await _userRepository
            .GetByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            throw new Exception("Email already exists");
        }

        var hashedPassword =
            BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            RoleId = dto.RoleId
        };

        await _userRepository.AddAsync(user);

        await _userRepository.SaveChangesAsync();

        return "User registered successfully";
    }

    public async Task<AuthResponseDto> LoginAsync(
     LoginRequestDto dto)
    {
        var user = await _userRepository
            .GetByEmailAsync(dto.Email);

        if (user == null)
        {
            throw new Exception("Invalid email");
        }

        var isPasswordValid =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new Exception("Invalid password");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.Name
        };
    }
}