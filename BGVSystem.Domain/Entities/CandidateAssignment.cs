using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Domain.Entities
{
    public class CandidateAssignment
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public Candidate Candidate { get; set; } = null!;

        public int ReviewerId { get; set; }

        public User Reviewer { get; set; } = null!;

        public DateTime AssignedDate { get; set; }
            = DateTime.UtcNow;
    }
}
