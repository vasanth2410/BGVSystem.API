using BGVSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);

        Task<string> LoginAsync(LoginRequestDto request);
    }
}
