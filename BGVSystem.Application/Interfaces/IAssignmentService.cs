using BGVSystem.Application.DTOs.Assignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IAssignmentService
    {
        Task<string>
            CreateAsync(
                CreateAssignmentDto dto);

        Task<List<AssignmentResponseDto>>
            GetAllAsync();

        Task<List<AssignmentResponseDto>>
            GetByReviewerIdAsync(int reviewerId);

        Task DeleteAsync(int id);

        Task<int> CleanupDuplicatesAsync();
    }
}
