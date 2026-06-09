using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.DTOs.Assignments
{
    public class AssignmentResponseDto
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public int ReviewerId { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}
