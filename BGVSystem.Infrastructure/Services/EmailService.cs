using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace BGVSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> options,
        INotificationRepository notificationRepository = null,
        ILogger<EmailService> logger = null)
    {
        _settings = options.Value;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task SendEmailAsync(SendEmailDto dto)
    {
        var notification = new Notification
        {
            ToEmail = dto.To,
            Subject = dto.Subject,
            Body = dto.Body,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        if (_notificationRepository != null)
        {
            try
            {
                await _notificationRepository.AddAsync(notification);
                await _notificationRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to persist initial notification to repository for {To}", dto.To);
            }
        }

        try
        {
            await SendEmailDirectAsync(notification);
            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SMTP transmission failed for {ToEmail}: {Message}", dto.To, ex.Message);
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;
        }

        if (_notificationRepository != null)
        {
            try
            {
                await _notificationRepository.UpdateAsync(notification);
                await _notificationRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to update notification status for {To}", dto.To);
            }
        }
    }

    public async Task SendEmailDirectAsync(Notification notification)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            var senderEmail = !string.IsNullOrWhiteSpace(_settings.Email) ? _settings.Email : _settings.SenderEmail;
            var displayName = !string.IsNullOrWhiteSpace(_settings.DisplayName) ? _settings.DisplayName : _settings.SenderName;
            var smtpHost = !string.IsNullOrWhiteSpace(_settings.Host) ? _settings.Host : _settings.SmtpServer;

            mimeMessage.From.Add(new MailboxAddress(displayName, senderEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(notification.ToEmail));
            mimeMessage.Subject = notification.Subject;

            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = notification.Body
            };

            using var client = new SmtpClient();

            var secureSocketOption = _settings.EnableSSL
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            var port = _settings.Port > 0 ? _settings.Port : 587;

            _logger?.LogInformation("Connecting to SMTP host {Host}:{Port} via MailKit...", smtpHost, port);

            await client.ConnectAsync(smtpHost, port, secureSocketOption);

            var cleanPassword = (_settings.Password ?? "").Replace(" ", "");
            if (!string.IsNullOrEmpty(senderEmail) && !string.IsNullOrEmpty(cleanPassword))
            {
                await client.AuthenticateAsync(senderEmail.Trim(), cleanPassword);
            }

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);

            _logger?.LogInformation("Email successfully dispatched to {ToEmail} with Subject: {Subject}", notification.ToEmail, notification.Subject);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "MailKit SmtpClient failed for {ToEmail}: {Message}", notification.ToEmail, ex.Message);
            Console.WriteLine($"[EMAIL NOTICE] Failed to send live SMTP email to {notification.ToEmail}: {ex.Message}");
            // Logged error gracefully - continue execution without crashing application
        }
    }

    #region 10 Enterprise Email Notification Methods

    // 1. Candidate Created
    public async Task SendWelcomeEmailAsync(string candidateEmail, string candidateName, string temporaryPassword)
    {
        var subject = "Welcome to BGV Portal";
        var message = $@"
            <p>Welcome to the <strong>Enterprise Background Verification (BGV) System</strong>.</p>
            <p>Your candidate portal account has been provisioned successfully. Below are your temporary access credentials:</p>
            <div style='background-color:#f1f5f9; border-left:4px solid #2563eb; padding:12px 16px; margin:16px 0; border-radius:4px;'>
                <p style='margin:4px 0;'><strong>Username:</strong> {candidateEmail}</p>
                <p style='margin:4px 0;'><strong>Temporary Password:</strong> <code style='font-family:monospace; background:#e2e8f0; padding:2px 6px; border-radius:4px; font-weight:bold; color:#1e293b;'>{temporaryPassword}</code></p>
            </div>
            <p>Please log in to complete your profile and submit the required verification documents.</p>";

        var html = BuildHtmlTemplate(
            title: "Welcome to BGV Portal",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "ACCOUNT CREATED",
            badgeColor: "#2563eb",
            actionButtonText: "Log In to BGV Portal",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 2. Admin Requests Documents
    public async Task SendDocumentRequestEmailAsync(string candidateEmail, string candidateName, string documentType, string remarks = "")
    {
        var subject = "Documents Required for Background Verification";
        var remarksBlock = string.IsNullOrWhiteSpace(remarks) ? "" : $"<p><strong>Notes / Instructions:</strong> {remarks}</p>";
        var message = $@"
            <p>We are initiating your background verification process and require specific documentation to proceed.</p>
            <div style='background-color:#f8fafc; border:1px solid #e2e8f0; padding:14px; margin:16px 0; border-radius:6px;'>
                <p style='margin:4px 0;'><strong>Requested Document(s):</strong> <span style='color:#1e40af; font-weight:600;'>{documentType}</span></p>
                {remarksBlock}
            </div>
            <p>Please log in to your portal and upload the required files at your earliest convenience.</p>";

        var html = BuildHtmlTemplate(
            title: "Action Required: Document Submission",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "ACTION REQUIRED",
            badgeColor: "#d97706",
            actionButtonText: "Upload Documents Now",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 3. Candidate Uploads Documents
    public async Task SendDocumentsUploadedEmailAsync(string candidateEmail, string candidateName)
    {
        var subject = "Documents Uploaded Successfully";
        var message = $@"
            <p>Thank you for submitting your verification documents.</p>
            <p>We have successfully received your uploaded files. Our background verification team has been notified and will review your submission shortly.</p>
            <p>No further action is required from your side at this moment.</p>";

        var html = BuildHtmlTemplate(
            title: "Documents Received",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "DOCUMENTS RECEIVED",
            badgeColor: "#059669",
            actionButtonText: "View Portal Status",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 4. Reviewer Starts Verification
    public async Task SendVerificationStartedEmailAsync(string candidateEmail, string candidateName, string verificationType)
    {
        var subject = "Background Verification Started";
        var message = $@"
            <p>This is an automated notification to inform you that your <strong>{verificationType}</strong> background verification process has officially commenced.</p>
            <p>Our verification specialists and primary sources are now validating the details provided. We will keep you updated on the progress.</p>";

        var html = BuildHtmlTemplate(
            title: "Verification In Progress",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "IN PROGRESS",
            badgeColor: "#0284c7",
            actionButtonText: "Track Verification",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 5. Reviewer Requests Additional Documents
    public async Task SendAdditionalDocumentsRequiredEmailAsync(string candidateEmail, string candidateName, string requestedDocuments, string remarks)
    {
        var subject = "Additional Documents Required";
        var message = $@"
            <p>During our review of your background verification file, our verification team identified additional document requirements.</p>
            <div style='background-color:#fff7ed; border-left:4px solid #f97316; padding:12px 16px; margin:16px 0; border-radius:4px;'>
                <p style='margin:4px 0;'><strong>Additional Document(s) Needed:</strong> {requestedDocuments}</p>
                <p style='margin:4px 0;'><strong>Reviewer Remarks:</strong> {remarks}</p>
            </div>
            <p>Please upload the updated or clear copies of these documents promptly to avoid verification delays.</p>";

        var html = BuildHtmlTemplate(
            title: "Additional Documents Needed",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "ATTENTION NEEDED",
            badgeColor: "#ea580c",
            actionButtonText: "Upload Additional Documents",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 6. Verification Completed
    public async Task SendVerificationCompletedEmailAsync(string candidateEmail, string candidateName, string overallStatus)
    {
        var subject = "Background Verification Completed";
        var message = $@"
            <p>We are pleased to inform you that your background verification checks have been fully completed by our verification team.</p>
            <p><strong>Overall Verification Outcome:</strong> <span style='font-weight:700; color:#1e293b;'>{overallStatus}</span></p>
            <p>The final report is being compiled for administrative review.</p>";

        var html = BuildHtmlTemplate(
            title: "Verification Process Completed",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "COMPLETED",
            badgeColor: "#16a34a",
            actionButtonText: "Check Final Details",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 7. Admin Approves Candidate
    public async Task SendVerificationApprovedEmailAsync(string candidateEmail, string candidateName, string remarks = "")
    {
        var subject = "Background Verification Approved";
        var remarksBlock = string.IsNullOrWhiteSpace(remarks) ? "" : $"<p><strong>Comments:</strong> {remarks}</p>";
        var message = $@"
            <p>Congratulations! Your background verification report has been thoroughly evaluated and <strong>APPROVED</strong> by the administration.</p>
            {remarksBlock}
            <p>All checks meet our compliance guidelines. Thank you for your cooperation throughout this process.</p>";

        var html = BuildHtmlTemplate(
            title: "Verification Approved",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "APPROVED",
            badgeColor: "#16a34a",
            actionButtonText: "Go to Portal",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 8. Admin Rejects Candidate
    public async Task SendVerificationRejectedEmailAsync(string candidateEmail, string candidateName, string reason)
    {
        var subject = "Background Verification Rejected";
        var message = $@"
            <p>We regret to inform you that your background verification status has been updated to <strong>REJECTED</strong>.</p>
            <div style='background-color:#fef2f2; border-left:4px solid #ef4444; padding:12px 16px; margin:16px 0; border-radius:4px;'>
                <p style='margin:4px 0;'><strong>Reason / Details:</strong> {reason}</p>
            </div>
            <p>If you believe this decision was made in error or wish to provide clarification, please reach out to our support team.</p>";

        var html = BuildHtmlTemplate(
            title: "Verification Decision Notice",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "REJECTED",
            badgeColor: "#dc2626",
            actionButtonText: "Contact Support",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 9. PDF Report Ready
    public async Task SendPdfReportReadyEmailAsync(string candidateEmail, string candidateName, string reportRefOrUrl)
    {
        var subject = "Your Verification Report is Ready";
        var message = $@"
            <p>Your official Background Verification Summary Report has been generated and is now ready for viewing and download.</p>
            <div style='background-color:#f8fafc; border:1px dashed #cbd5e1; padding:12px 16px; margin:16px 0; border-radius:6px;'>
                <p style='margin:4px 0;'><strong>Report Reference:</strong> {reportRefOrUrl}</p>
            </div>
            <p>You can access your PDF report directly from your candidate portal.</p>";

        var html = BuildHtmlTemplate(
            title: "Verification Report Ready",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "REPORT READY",
            badgeColor: "#4f46e5",
            actionButtonText: "Download PDF Report",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    // 10. Forgot Password
    public async Task SendPasswordResetEmailAsync(string candidateEmail, string candidateName, string resetTokenOrLink)
    {
        var subject = "Reset Your Password";
        var message = $@"
            <p>We received a request to reset the password for your BGV System account.</p>
            <p>Use the temporary security code / link below to reset your credentials:</p>
            <div style='background-color:#f1f5f9; border-left:4px solid #6366f1; padding:12px 16px; margin:16px 0; border-radius:4px;'>
                <p style='margin:4px 0;'><strong>Reset Reference / Code:</strong> <code style='font-family:monospace; font-weight:bold; color:#4338ca;'>{resetTokenOrLink}</code></p>
            </div>
            <p>If you did not request a password reset, please ignore this email or contact support immediately.</p>";

        var html = BuildHtmlTemplate(
            title: "Password Reset Request",
            candidateName: candidateName,
            messageHtml: message,
            badgeText: "SECURITY ALERT",
            badgeColor: "#4f46e5",
            actionButtonText: "Reset Password Now",
            actionButtonUrl: "#"
        );

        await SendEmailAsync(new SendEmailDto { To = candidateEmail, Subject = subject, Body = html });
    }

    #endregion

    #region HTML Template Builder

    private string BuildHtmlTemplate(
        string title,
        string candidateName,
        string messageHtml,
        string badgeText,
        string badgeColor,
        string actionButtonText = null,
        string actionButtonUrl = null)
    {
        var buttonHtml = string.IsNullOrWhiteSpace(actionButtonText) ? "" : $@"
            <div style='text-align: center; margin: 28px 0 16px 0;'>
                <a href='{(string.IsNullOrWhiteSpace(actionButtonUrl) ? "#" : actionButtonUrl)}' 
                   style='background-color: #1e40af; color: #ffffff; text-decoration: none; padding: 12px 28px; font-weight: 600; font-size: 14px; border-radius: 6px; display: inline-block; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                   {actionButtonText}
                </a>
            </div>";

        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; color: #334155; -webkit-font-smoothing: antialiased;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f1f5f9; padding: 30px 15px;'>
        <tr>
            <td align='center'>
                <!-- Main Container -->
                <table role='presentation' width='100%' style='max-width: 600px; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); border: 1px solid #e2e8f0;' cellspacing='0' cellpadding='0'>
                    
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%); padding: 24px 32px; text-align: left;'>
                            <table width='100%' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td>
                                        <!-- Company Logo Placeholder -->
                                        <div style='display: inline-block; background-color: rgba(255, 255, 255, 0.15); padding: 8px 14px; border-radius: 6px; border: 1px solid rgba(255, 255, 255, 0.2);'>
                                            <span style='color: #ffffff; font-weight: 800; font-size: 18px; letter-spacing: 1px;'>🛡️ BGV System</span>
                                        </div>
                                    </td>
                                    <td align='right'>
                                        <span style='background-color: {badgeColor}; color: #ffffff; font-size: 10px; font-weight: 700; padding: 4px 10px; border-radius: 20px; text-transform: uppercase; letter-spacing: 0.5px;'>
                                            {badgeText}
                                        </span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Title Banner -->
                    <tr>
                        <td style='padding: 24px 32px 12px 32px; border-bottom: 1px solid #f1f5f9;'>
                            <h2 style='margin: 0; color: #0f172a; font-size: 20px; font-weight: 700; line-height: 1.3;'>{title}</h2>
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td style='padding: 24px 32px; font-size: 15px; line-height: 1.6; color: #334155;'>
                            <p style='margin-top: 0; font-weight: 600; color: #1e293b;'>Dear {candidateName},</p>
                            {messageHtml}
                            {buttonHtml}
                            <p style='margin-bottom: 0; color: #64748b; font-size: 14px;'>
                                Best regards,<br>
                                <strong style='color: #1e293b;'>BGV System Verification Team</strong>
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8fafc; padding: 20px 32px; border-top: 1px solid #e2e8f0; text-align: center; font-size: 12px; color: #94a3b8;'>
                            <p style='margin: 0 0 6px 0; font-weight: 500;'>Enterprise Background Verification System</p>
                            <p style='margin: 0 0 6px 0;'>Need assistance? Contact us at <a href='mailto:support@bgvsystem.com' style='color: #2563eb; text-decoration: none;'>support@bgvsystem.com</a></p>
                            <p style='margin: 0; color: #cbd5e1; font-size: 11px;'>This is an automated notification. Please do not reply directly to this email.</p>
                        </td>
                    </tr>

                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    #endregion
}