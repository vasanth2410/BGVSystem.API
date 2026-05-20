using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.DTOs.Document
{
    public class UploadDocumentDto
    {
        public int CandidateId { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}
