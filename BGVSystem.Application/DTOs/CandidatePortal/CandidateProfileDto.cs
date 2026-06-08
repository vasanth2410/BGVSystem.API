using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.CandidatePortal
{
    public class CandidateProfileDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string AppliedRole { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
