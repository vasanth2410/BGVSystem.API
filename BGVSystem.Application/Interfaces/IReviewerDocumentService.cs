using BGVSystem.Application.DTOs.ReviewerDocuments;

namespace BGVSystem.Application.Interfaces;

public interface IReviewerDocumentService
{
    Task<ReviewerDashboardDto> GetDashboardAsync();

    Task<List<ReviewerDocumentDto>> GetPendingDocumentsAsync();

    Task<List<ReviewerDocumentDto>> GetApprovedDocumentsAsync();

    Task<List<ReviewerDocumentDto>> GetRejectedDocumentsAsync();

    Task<ReviewerDocumentDto?> GetDocumentAsync(int documentId);

    Task<string> ApproveDocumentAsync(int documentId);

    Task<string> RejectDocumentAsync(int documentId);
}