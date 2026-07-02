using BGVSystem.Domain.Entities;

using BGVSystem.Application.DTOs.Candidates;

namespace BGVSystem.Application.Interfaces;

public interface ICandidateRepository
{
    Task<List<Candidate>> GetAllAsync();

    Task<Candidate?> GetByIdAsync(int id);

    Task AddAsync(Candidate candidate);

    Task UpdateAsync(Candidate candidate);

    Task DeleteAsync(Candidate candidate);

    Task SaveChangesAsync();

    Task<Candidate?> GetByEmailAsync(
    string email);

    //Task<int> GetCountAsync();
    Task<int> GetTotalCountAsync();

    Task<List<Candidate>> GetByStatusAsync(string status);

    Task<List<Candidate>> SearchAsync(CandidateSearchDto dto);

    Task<List<Candidate>> GetDeletedCandidatesAsync();

    Task RestoreAsync(int id);

    Task PermanentDeleteAsync(int id);

    Task<List<Candidate>> GetRecentCandidatesAsync();
}