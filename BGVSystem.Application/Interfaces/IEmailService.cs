using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(SendEmailDto dto);

        Task SendEmailDirectAsync(Notification notification);

        // 1. Candidate Created
        Task SendWelcomeEmailAsync(string candidateEmail, string candidateName, string temporaryPassword);

        // 2. Admin Requests Documents
        Task SendDocumentRequestEmailAsync(string candidateEmail, string candidateName, string documentType, string remarks = "");

        // 3. Candidate Uploads Documents
        Task SendDocumentsUploadedEmailAsync(string candidateEmail, string candidateName);

        // 4. Reviewer Starts Verification
        Task SendVerificationStartedEmailAsync(string candidateEmail, string candidateName, string verificationType);

        // 5. Reviewer Requests Additional Documents
        Task SendAdditionalDocumentsRequiredEmailAsync(string candidateEmail, string candidateName, string requestedDocuments, string remarks);

        // 6. Verification Completed
        Task SendVerificationCompletedEmailAsync(string candidateEmail, string candidateName, string overallStatus);

        // 7. Admin Approves Candidate
        Task SendVerificationApprovedEmailAsync(string candidateEmail, string candidateName, string remarks = "");

        // 8. Admin Rejects Candidate
        Task SendVerificationRejectedEmailAsync(string candidateEmail, string candidateName, string reason);

        // 9. PDF Report Generated
        Task SendPdfReportReadyEmailAsync(string candidateEmail, string candidateName, string reportRefOrUrl);

        // 10. Forgot Password
        Task SendPasswordResetEmailAsync(string candidateEmail, string candidateName, string resetTokenOrLink);
    }
}
