using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BGVSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly INotificationRepository
    _notificationRepository;
    public EmailService(
     IOptions<EmailSettings> options,
     INotificationRepository notificationRepository)
    {
        _settings = options.Value;

        _notificationRepository =
            notificationRepository;
    }

    public async Task SendEmailAsync(
    SendEmailDto dto)
    {
        var notification =
            new Notification
            {
                ToEmail = dto.To,

                Subject = dto.Subject,

                Body = dto.Body,

                Status = "Pending",

                CreatedAt = DateTime.UtcNow
            };

        await _notificationRepository
            .AddAsync(notification);

        await _notificationRepository
            .SaveChangesAsync();

        try
        {
            using var message =
                new MailMessage();

            message.From =
                new MailAddress(
                    _settings.SenderEmail,
                    _settings.SenderName);

            message.To.Add(dto.To);

            message.Subject =
                dto.Subject;

            message.Body =
                dto.Body;

            message.IsBodyHtml = true;

            using var client =
                new SmtpClient(
                    _settings.SmtpServer,
                    _settings.Port);

            client.Credentials =
                new NetworkCredential(
                    _settings.Username,
                    _settings.Password);

            client.EnableSsl = true;

            await client.SendMailAsync(message);

            notification.Status = "Sent";

            notification.SentAt =
                DateTime.UtcNow;

            await _notificationRepository
                .UpdateAsync(notification);

            await _notificationRepository
                .SaveChangesAsync();
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";

            notification.ErrorMessage =
                ex.Message;

            await _notificationRepository
                .UpdateAsync(notification);

            await _notificationRepository
                .SaveChangesAsync();

            throw;
        }
    }

    public async Task SendEmailDirectAsync(
    Notification notification)
    {
        using var message =
            new MailMessage();

        message.From =
            new MailAddress(
                _settings.SenderEmail,
                _settings.SenderName);

        message.To.Add(
            notification.ToEmail);

        message.Subject =
            notification.Subject;

        message.Body =
            notification.Body;

        message.IsBodyHtml = true;

        using var client =
            new SmtpClient(
                _settings.SmtpServer,
                _settings.Port);

        client.Credentials =
            new NetworkCredential(
                _settings.Username,
                _settings.Password);

        client.EnableSsl = true;

        await client.SendMailAsync(message);
    }
}