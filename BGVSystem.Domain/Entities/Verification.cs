using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Domain.Entities
{
    public class Verification
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string VerificationType { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string ReviewerRemarks { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Candidate Candidate { get; set; } = null!;
    }
}
