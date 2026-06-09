using BGVSystem.Application.DTOs.AdminDashBoard;
using BGVSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Services
{
    public class AdminDashboardService:IAdminDashboardService
    {
        private readonly ICandidateRepository _candidateRepository;
        public AdminDashboardService(ICandidateRepository candidateRepository) 
        {
            _candidateRepository = candidateRepository;
        }

        public async Task<List<CandidateWorkQueueDto>>
    GetPendingCandidatesAsync()
        {
            var candidates =
                await _candidateRepository
                    .GetByStatusAsync("Pending");

            return candidates
                .Select(x =>
                    new CandidateWorkQueueDto
                    {
                        CandidateId = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        Status = x.Status
                    })
                .ToList();
        }

        public async Task<List<CandidateWorkQueueDto>>
    GetCompletedCandidatesAsync()
        {
            var candidates =
                await _candidateRepository
                    .GetByStatusAsync("Completed");

            return candidates
                .Select(x =>
                    new CandidateWorkQueueDto
                    {
                        CandidateId = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        Status = x.Status
                    })
                .ToList();
        }

        public async Task<List<CandidateWorkQueueDto>>
    GetRejectedCandidatesAsync()
        {
            var candidates =
                await _candidateRepository
                    .GetByStatusAsync("Rejected");

            return candidates
                .Select(x =>
                    new CandidateWorkQueueDto
                    {
                        CandidateId = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        Status = x.Status
                    })
                .ToList();
        }
    }
}
