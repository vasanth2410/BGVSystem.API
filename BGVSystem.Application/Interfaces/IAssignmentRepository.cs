using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IAssignmentRepository
    {
        Task AddAsync(
            CandidateAssignment assignment);

        Task<List<CandidateAssignment>>
            GetAllAsync();

        Task<List<CandidateAssignment>>
            GetByReviewerIdAsync(int reviewerId);

        Task SaveChangesAsync();

        
    }
}
