using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Candidates
{
    public class CandidateSearchDto
    {
        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
