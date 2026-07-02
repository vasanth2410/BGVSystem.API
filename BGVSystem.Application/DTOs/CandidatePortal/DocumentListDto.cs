using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.CandidatePortal
{
    public class DocumentListDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}
