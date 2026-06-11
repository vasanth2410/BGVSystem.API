using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.AdminDashBoard
{
    public class DashboardSummaryDto
    {
        public int TotalCandidates { get; set; }

        public int PendingCandidates { get; set; }

        public int CompletedCandidates { get; set; }

        public int RejectedCandidates { get; set; }
    }
}
