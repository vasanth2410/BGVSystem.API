using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.DTOs.Verifications
{
    public class CreateVerificationDto
    {
        public int CandidateId { get; set; }

        public string VerificationType { get; set; } = string.Empty;
    }
}
