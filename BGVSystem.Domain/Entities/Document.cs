using System;
using System.Collections.Generic;
using System.Text;


namespace BGVSystem.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public Candidate Candidate { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string Status { get; set; } = "Uploaded";

        public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    }
}
