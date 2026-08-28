using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Domain.Entities
{

    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool MustChangePassword { get; set; } = false;
    }
}
