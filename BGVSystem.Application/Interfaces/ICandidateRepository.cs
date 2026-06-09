using BGVSystem.Domain.Entities;

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

    Task<int> GetCountAsync();

    Task<List<Candidate>> GetByStatusAsync(string status);
}