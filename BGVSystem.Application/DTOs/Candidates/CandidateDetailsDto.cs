using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Candidates
{
    public class CandidateDetailsDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string PANNumber { get; set; } = string.Empty;

        public string AadhaarNumber { get; set; } = string.Empty;

        public string AppliedRole { get; set; } = string.Empty;

        public DateTime DateOfJoining { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
