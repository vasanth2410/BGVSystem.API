using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Infrastructure.Settings
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string DisplayName { get; set; } = "BGV System";

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool EnableSSL { get; set; } = true;

        public string ResendApiKey { get; set; } = string.Empty;

        public string BrevoApiKey { get; set; } = string.Empty;

        public string FrontendUrl { get; set; } = "https://bgv-project-frontend.vercel.app";

        // Backwards-compatibility property aliases
        public string SmtpServer
        {
            get => !string.IsNullOrEmpty(Host) ? Host : string.Empty;
            set => Host = value;
        }

        public string SenderEmail
        {
            get => !string.IsNullOrEmpty(Email) ? Email : string.Empty;
            set => Email = value;
        }

        public string SenderName
        {
            get => !string.IsNullOrEmpty(DisplayName) ? DisplayName : string.Empty;
            set => DisplayName = value;
        }

        public string Username
        {
            get => !string.IsNullOrEmpty(Email) ? Email : string.Empty;
            set => Email = value;
        }
    }
}
