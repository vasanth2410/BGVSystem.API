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
        Task SendEmailAsync(
            SendEmailDto dto);

        Task SendEmailDirectAsync(
    Notification notification);
    }
}
