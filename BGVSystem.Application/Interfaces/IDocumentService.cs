using BGVSystem.Application.DTOs.Document;
using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Interfaces;

public interface IDocumentService
{
    Task<string> UploadAsync(UploadDocumentDto dto);

    Task<List<DocumentResponseDto>> GetByCandidateIdAsync(int candidateId);

    Task<List<DocumentResponseDto>> GetAllAsync();

    Task<string> DeleteAsync(int id);

    Task<Document?> GetDocumentByIdAsync(int id);
}