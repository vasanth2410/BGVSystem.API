using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Assignments
{
    public class CreateAssignmentDto
    {
        public int CandidateId { get; set; }

        public int ReviewerId { get; set; }
    }
}
