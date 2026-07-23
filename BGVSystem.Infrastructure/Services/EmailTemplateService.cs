using BGVSystem.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BGVSystem.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public async Task<string> GetWelcomeTemplateAsync(
            string fullName,
            string email,
            string password)
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "EmailTemplates",
                "WelcomeCandidate.html");

            if (File.Exists(path))
            {
                var html = await File.ReadAllTextAsync(path);
                return html
                    .Replace("{{FullName}}", fullName)
                    .Replace("{{Email}}", email)
                    .Replace("{{Password}}", password);
            }

            // Fallback rich HTML template
            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #060d26; color: #ffffff; padding: 20px; }}
  .container {{ max-width: 580px; margin: 0 auto; background: #0b1739; border: 1px solid #1e3a8a; border-radius: 12px; padding: 30px; }}
  .header {{ font-size: 22px; font-weight: bold; color: #38bdf8; margin-bottom: 20px; border-bottom: 1px solid #1e3a8a; padding-bottom: 12px; }}
  .content {{ font-size: 15px; line-height: 1.6; color: #cbd5e1; }}
  .cred-box {{ background: #061233; border: 1px solid #0284c7; border-radius: 8px; padding: 16px; margin: 20px 0; }}
  .cred-item {{ margin: 6px 0; font-size: 14px; }}
  .btn {{ display: inline-block; background: linear-gradient(135deg, #2563eb, #0284c7); color: #ffffff !important; padding: 12px 26px; border-radius: 8px; text-decoration: none; font-weight: bold; margin-top: 15px; }}
</style>
</head>
<body>
  <div class='container'>
    <div class='header'>🛡️ Welcome to BGV System</div>
    <div class='content'>
      <p>Hello <b>{fullName}</b>,</p>
      <p>Your Background Verification onboarding profile has been created successfully. Please log into the portal to complete your document submission.</p>
      <div class='cred-box'>
        <div class='cred-item'>📧 Email: <b>{email}</b></div>
        <div class='cred-item'>🔑 Temporary Password: <b>{password}</b></div>
      </div>
      <p>Click below to log in and change your password:</p>
      <a href='http://localhost:5173/login' class='btn'>Login to BGV Portal →</a>
      <p style='margin-top: 25px; font-size: 13px; color: #64748b;'>Regards,<br/><b>BGV Operations Team</b></p>
    </div>
  </div>
</body>
</html>";
        }

        public async Task<string> GetDocumentRequestTemplateAsync(
            string fullName,
            string documentType,
            string remarks)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #060d26; color: #ffffff; padding: 20px; }}
  .container {{ max-width: 580px; margin: 0 auto; background: #0b1739; border: 1px solid #0284c7; border-radius: 12px; padding: 30px; }}
  .header {{ font-size: 20px; font-weight: bold; color: #f59e0b; margin-bottom: 20px; border-bottom: 1px solid #1e3a8a; padding-bottom: 12px; }}
  .content {{ font-size: 15px; line-height: 1.6; color: #cbd5e1; }}
  .alert-box {{ background: rgba(245, 158, 11, 0.15); border-left: 4px solid #f59e0b; border-radius: 6px; padding: 14px; margin: 20px 0; }}
  .btn {{ display: inline-block; background: linear-gradient(135deg, #f59e0b, #d97706); color: #ffffff !important; padding: 12px 26px; border-radius: 8px; text-decoration: none; font-weight: bold; margin-top: 15px; }}
</style>
</head>
<body>
  <div class='container'>
    <div class='header'>📄 Action Required: Document Resubmission Notice</div>
    <div class='content'>
      <p>Dear <b>{fullName}</b>,</p>
      <p>Your BGV Verifier has requested an update/resubmission for your document: <b>{documentType}</b>.</p>
      <div class='alert-box'>
        <b>Verifier Remarks:</b><br/>
        <i>{remarks}</i>
      </div>
      <p>Please log into your candidate portal to upload a clear copy of the requested document.</p>
      <a href='http://localhost:5173/login' class='btn'>Upload Document Now →</a>
      <p style='margin-top: 25px; font-size: 13px; color: #64748b;'>Regards,<br/><b>BGV Verification Team</b></p>
    </div>
  </div>
</body>
</html>";
        }

        public async Task<string> GetStatusUpdateTemplateAsync(
            string fullName,
            string status,
            string remarks)
        {
            string statusColor = status switch
            {
                "Completed" or "Approved" or "Cleared" => "#22c55e",
                "Rejected" or "Failed" => "#ef4444",
                "On Hold" => "#f59e0b",
                _ => "#38bdf8"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
  body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #060d26; color: #ffffff; padding: 20px; }}
  .container {{ max-width: 580px; margin: 0 auto; background: #0b1739; border: 1px solid #1e3a8a; border-radius: 12px; padding: 30px; }}
  .header {{ font-size: 20px; font-weight: bold; color: {statusColor}; margin-bottom: 20px; border-bottom: 1px solid #1e3a8a; padding-bottom: 12px; }}
  .content {{ font-size: 15px; line-height: 1.6; color: #cbd5e1; }}
  .status-badge {{ display: inline-block; background: {statusColor}; color: #ffffff; padding: 6px 16px; border-radius: 20px; font-weight: bold; font-size: 14px; margin: 10px 0; }}
  .remarks-box {{ background: #061233; border: 1px solid #1e3a8a; border-radius: 8px; padding: 14px; margin: 15px 0; font-size: 14px; }}
  .btn {{ display: inline-block; background: linear-gradient(135deg, #2563eb, #0284c7); color: #ffffff !important; padding: 12px 26px; border-radius: 8px; text-decoration: none; font-weight: bold; margin-top: 15px; }}
</style>
</head>
<body>
  <div class='container'>
    <div class='header'>🔔 BGV Status Alert: {status}</div>
    <div class='content'>
      <p>Hello <b>{fullName}</b>,</p>
      <p>Your Background Verification status has been updated to:</p>
      <div class='status-badge'>{status}</div>
      {(string.IsNullOrWhiteSpace(remarks) ? "" : $"<div class='remarks-box'><b>Remarks:</b> {remarks}</div>")}
      <p>You can check the full details of your verification process by logging into your portal account.</p>
      <a href='http://localhost:5173/login' class='btn'>View Status Portal →</a>
      <p style='margin-top: 25px; font-size: 13px; color: #64748b;'>Regards,<br/><b>BGV Operations Team</b></p>
    </div>
  </div>
</body>
</html>";
        }
    }
}