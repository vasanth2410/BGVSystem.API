using BGVSystem.Application.DTOs.AdminDashBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<List<CandidateWorkQueueDto>> GetPendingCandidatesAsync();

        Task<List<CandidateWorkQueueDto>> GetCompletedCandidatesAsync();

        Task<List<CandidateWorkQueueDto>> GetRejectedCandidatesAsync();

        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
