using BGVSystem.Application.DTOs.Verifications;

namespace BGVSystem.Application.Interfaces;

public interface IOcrService
{
    Task<OcrResultDto> ProcessDocumentOcrAsync(int documentId, string fileName, string filePath);
}
