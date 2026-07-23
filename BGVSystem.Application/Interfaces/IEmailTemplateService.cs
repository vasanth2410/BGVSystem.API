using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<string> GetWelcomeTemplateAsync(
            string fullName,
            string email,
            string password);

        Task<string> GetDocumentRequestTemplateAsync(
            string fullName,
            string documentType,
            string remarks);

        Task<string> GetStatusUpdateTemplateAsync(
            string fullName,
            string status,
            string remarks);
    }
}
