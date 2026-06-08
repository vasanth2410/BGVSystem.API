using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Reviewer
{
    public class ReviewerDashboardDto
    {
        public int TotalCandidates { get; set; }

        public int TotalDocuments { get; set; }

        public int PendingVerifications { get; set; }

        public int ApprovedVerifications { get; set; }

        public int RejectedVerifications { get; set; }
    }
}
