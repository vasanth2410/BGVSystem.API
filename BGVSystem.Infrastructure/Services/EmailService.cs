using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BGVSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> options,
        INotificationRepository notificationRepository,
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

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        try
        {
            await SendEmailDirectAsync(notification);
            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SMTP live transmission failed for {ToEmail}: {Message}", dto.To, ex.Message);
            Console.WriteLine($"\n========================================================");
            Console.WriteLine($"❌ [REAL EMAIL DISPATCH FAILED]");
            Console.WriteLine($"TO: {dto.To}");
            Console.WriteLine($"SUBJECT: {dto.Subject}");
            Console.WriteLine($"REASON: {ex.Message}");
            Console.WriteLine($"========================================================\n");
            
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;
        }

        await _notificationRepository.UpdateAsync(notification);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task SendEmailDirectAsync(Notification notification)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
            message.To.Add(notification.ToEmail);
            message.Subject = notification.Subject;
            message.Body = notification.Body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port);
            client.UseDefaultCredentials = false;
            var cleanPassword = (_settings.Password ?? "").Replace(" ", "");
            client.Credentials = new NetworkCredential(_settings.Username.Trim(), cleanPassword);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("SMTP SendMailAsync failed: {Message}. Logging notification locally.", ex.Message);
            Console.WriteLine($"[EMAIL SIMULATION DIRECT] To: {notification.ToEmail} | Subject: {notification.Subject}");
            // Re-throw so worker can mark status appropriately or complete cleanly
            throw;
        }
    }
}