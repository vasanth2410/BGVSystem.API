using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.ReviewerDocuments
{
    public class ReviewerDocumentDto
    {
        public int DocumentId { get; set; }

        public int CandidateId { get; set; }

        public string CandidateName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}
