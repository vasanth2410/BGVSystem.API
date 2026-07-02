using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Reviewer
{
    public class ReviewDocumentDto
    {
        public string Status { get; set; } = string.Empty;

        public string ReviewerRemarks { get; set; } = string.Empty;
    }
}
