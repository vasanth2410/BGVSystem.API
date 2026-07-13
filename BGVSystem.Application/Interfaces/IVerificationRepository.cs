using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IVerificationRepository
    {
        Task AddAsync(Verification verification);

        Task<List<Verification>> GetAllAsync();

        Task<Verification?> GetByIdAsync(int id);

        Task SaveChangesAsync();

        Task<List<Verification>> GetByStatusAsync(string status);

        Task<int> GetCountAsync();

        Task<int> GetCountByStatusAsync(string status);

        Task<List<Verification>> GetByCandidateIdAsync(int candidateId);

        // ===========================
        // Dashboard Statistics
        // ===========================

        Task<int> GetAssignedCountAsync();

        Task<int> GetPendingCountAsync();

        Task<int> GetApprovedCountAsync();

        Task<int> GetRejectedCountAsync();

        Task<Verification?> GetByCandidateAndTypeAsync(
    int candidateId,
    string verificationType);

        Task<List<Verification>>
GetByCandidateIdsAsync(
    List<int> candidateIds);
    }
}
