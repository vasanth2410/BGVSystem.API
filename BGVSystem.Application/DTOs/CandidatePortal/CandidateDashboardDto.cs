using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.CandidatePortal
{
    public class CandidateDashboardDto
    {
        public string CandidateName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int UploadedDocuments { get; set; }

        public int RequiredDocuments { get; set; }
    }
}
