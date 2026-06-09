using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document);

    Task<List<Document>> GetByCandidateIdAsync(int candidateId);

    Task<Document?> GetByIdAsync(int id);

    Task DeleteAsync(Document document);


    Task<List<Document>> GetAllAsync();

    Task SaveChangesAsync();

    Task<int> GetCountAsync();
}