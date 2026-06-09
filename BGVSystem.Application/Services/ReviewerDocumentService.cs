using BGVSystem.Application.DTOs.ReviewerDocuments;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class ReviewerDocumentService
    : IReviewerDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    private readonly ICandidateRepository _candidateRepository;

    public ReviewerDocumentService(
        IDocumentRepository documentRepository,
        ICandidateRepository candidateRepository)
    {
        _documentRepository = documentRepository;
        _candidateRepository = candidateRepository;
    }

    public async Task<ReviewerDashboardDto>
        GetDashboardAsync()
    {
        var allDocuments =
            await _documentRepository
                .GetAllAsync();

        return new ReviewerDashboardDto
        {
            TotalDocuments =
                allDocuments.Count,

            PendingDocuments =
                allDocuments.Count(x =>
                    x.Status == "Pending"),

            ApprovedDocuments =
                allDocuments.Count(x =>
                    x.Status == "Approved"),

            RejectedDocuments =
                allDocuments.Count(x =>
                    x.Status == "Rejected")
        };
    }

    public async Task<List<ReviewerDocumentDto>>
        GetPendingDocumentsAsync()
    {
        var documents =
            await _documentRepository.GetAllAsync();

        documents = documents
            .Where(x => x.Status == "Pending")
            .ToList();

        return await MapDocuments(documents);
    }

    public async Task<List<ReviewerDocumentDto>>
        GetApprovedDocumentsAsync()
    {
        var documents =
            await _documentRepository.GetAllAsync();

        documents = documents
            .Where(x => x.Status == "Approved")
            .ToList();

        return await MapDocuments(documents);
    }

    public async Task<List<ReviewerDocumentDto>>
        GetRejectedDocumentsAsync()
    {
        var documents =
            await _documentRepository.GetAllAsync();

        documents = documents
            .Where(x => x.Status == "Rejected")
            .ToList();

        return await MapDocuments(documents);
    }

    public async Task<ReviewerDocumentDto?>
        GetDocumentAsync(int documentId)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(documentId);

        if (document == null)
            return null;

        var candidate =
            await _candidateRepository
                .GetByIdAsync(document.CandidateId);

        return new ReviewerDocumentDto
        {
            DocumentId = document.Id,
            CandidateId = document.CandidateId,
            CandidateName =
                candidate?.FullName ?? "",
            FileName = document.OriginalFileName,
            FileType = document.FileType,
            Status = document.Status,
            UploadedDate = document.UploadedDate
        };
    }

    public async Task<string>
        ApproveDocumentAsync(int documentId)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(documentId);

        if (document == null)
            throw new Exception(
                "Document not found");

        document.Status = "Approved";

        await _documentRepository
            .SaveChangesAsync();

        return "Document Approved";
    }

    public async Task<string>
        RejectDocumentAsync(int documentId)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(documentId);

        if (document == null)
            throw new Exception(
                "Document not found");

        document.Status = "Rejected";

        await _documentRepository
            .SaveChangesAsync();

        return "Document Rejected";
    }

    private async Task<List<ReviewerDocumentDto>>
        MapDocuments(List<Domain.Entities.Document> docs)
    {
        var result =
            new List<ReviewerDocumentDto>();

        foreach (var document in docs)
        {
            var candidate =
                await _candidateRepository
                    .GetByIdAsync(document.CandidateId);

            result.Add(
                new ReviewerDocumentDto
                {
                    DocumentId = document.Id,
                    CandidateId = document.CandidateId,
                    CandidateName =
                        candidate?.FullName ?? "",

                    FileName =
                        document.OriginalFileName,

                    FileType =
                        document.FileType,

                    Status =
                        document.Status,

                    UploadedDate =
                        document.UploadedDate
                });
        }

        return result;
    }
}