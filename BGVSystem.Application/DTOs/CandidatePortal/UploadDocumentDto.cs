using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.CandidatePortal
{
    public class UploadDocumentDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
