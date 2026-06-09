using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.ReviewerDocuments
{
    public class ReviewerDashboardDto
    {
        public int TotalDocuments { get; set; }

        public int PendingDocuments { get; set; }

        public int ApprovedDocuments { get; set; }

        public int RejectedDocuments { get; set; }
    }
}
