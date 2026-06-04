using BGVSystem.Application.Interfaces;

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

            Console.WriteLine($"Template Path: {path}");

            var html =
                await File.ReadAllTextAsync(path);

            Console.WriteLine(html);

            html = html.Replace(
                "{{FullName}}",
                fullName);

            html = html.Replace(
                "{{Email}}",
                email);

            html = html.Replace(
                "{{Password}}",
                password);

            return html;
        }
    }
}