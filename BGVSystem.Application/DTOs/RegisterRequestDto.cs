using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.DTOs
{
    public class RegisterRequestDto
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int RoleId { get; set; }
    }
}
