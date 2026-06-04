using BGVSystem.Application.DTOs.Auth;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Application.Exceptions;

namespace BGVSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    private readonly IJwtService _jwtService;

    private readonly IAuditService _auditService;

    public AuthService(
    IUserRepository userRepository,
    IJwtService jwtService,
    IAuditService auditService)
    {
        _userRepository = userRepository;

        _jwtService = jwtService;

        _auditService = auditService;
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
            throw new UnauthorizedException(
     "Invalid email or password");
        }

        var isPasswordValid =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedException(
    "Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        // Audit Log

        await _auditService.AddLogAsync(
            "User Logged In",
            user.Email,
            user.Role.Name);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.Name
        };
    }
}