using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.DTOs.Document
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
