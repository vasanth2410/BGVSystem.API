using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Reviewer
{
    public class ReviewerDashboardDto
    {
        public int Assigned { get; set; }

        public int Pending { get; set; }

        public int Approved { get; set; }

        public int Rejected { get; set; }

        public double CompletionPercentage { get; set; }
    }
}
