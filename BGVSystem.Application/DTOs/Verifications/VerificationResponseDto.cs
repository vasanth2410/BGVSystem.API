using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.DTOs.Verifications
{
    public class VerificationResponseDto
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string VerificationType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ReviewerRemarks { get; set; } = string.Empty;
    }
}
