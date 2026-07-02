using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Reviewer
{
    public class UpdateReviewerProfileDto
    {
        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Department { get; set; }

        public string? Designation { get; set; }
    }
}
