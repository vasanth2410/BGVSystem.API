using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.CandidatePortal
{
    public class DocumentDownloadDto
    {
        public byte[] FileBytes { get; set; } = [];

        public string ContentType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
    }
}
