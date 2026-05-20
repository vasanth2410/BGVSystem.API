using BGVSystem.Application.DTOs.Document;
//using BGVSystem.Application.DTOs.Documents;

namespace BGVSystem.Application.Interfaces;

public interface IDocumentService
{
    Task<string> UploadAsync(UploadDocumentDto dto);

    Task<List<DocumentResponseDto>> GetByCandidateIdAsync(int candidateId);

    Task<string> DeleteAsync(int id);
}