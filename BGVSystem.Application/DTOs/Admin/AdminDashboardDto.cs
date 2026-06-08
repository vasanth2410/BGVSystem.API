using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalCandidates { get; set; }

        public int TotalDocuments { get; set; }

        public int TotalVerifications { get; set; }

        public int PendingVerifications { get; set; }

        public int ApprovedVerifications { get; set; }

        public int RejectedVerifications { get; set; }

        public int NotificationsSent { get; set; }

        public int AuditLogsCount { get; set; }
    }
}
