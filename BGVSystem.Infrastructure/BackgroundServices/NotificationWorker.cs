using BGVSystem.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Infrastructure.BackgroundServices
{
    public class NotificationWorker
    : BackgroundService
    {
        private readonly IServiceScopeFactory
            _serviceScopeFactory;

        public NotificationWorker(
            IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory =
                serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope =
                    _serviceScopeFactory
                        .CreateScope();

                var notificationRepository =
                    scope.ServiceProvider
                        .GetRequiredService<
                            INotificationRepository>();

                var emailService =
                    scope.ServiceProvider
                        .GetRequiredService<
                            IEmailService>();

                var pendingNotifications =
                    await notificationRepository
                        .GetPendingAsync();

                foreach (var notification
                    in pendingNotifications)
                {
                    try
                    {
                        await emailService
                            .SendEmailDirectAsync(
                                notification);

                        notification.Status =
                            "Sent";

                        notification.SentAt =
                            DateTime.UtcNow;

                        await notificationRepository
                            .UpdateAsync(notification);
                    }
                    catch (Exception ex)
                    {
                        notification.RetryCount++;

                        notification.LastAttemptAt =
                            DateTime.UtcNow;

                        notification.ErrorMessage =
                            ex.Message;

                        if (notification.RetryCount
                            >= notification.MaxRetryCount)
                        {
                            notification.Status =
                                "DeadLetter";
                        }
                        else
                        {
                            notification.Status =
                                "Pending";
                        }

                        await notificationRepository
                            .UpdateAsync(notification);
                    }
                }

                await notificationRepository
                    .SaveChangesAsync();

                await Task.Delay(
                    TimeSpan.FromSeconds(10),
                    stoppingToken);
            }
        }
    }
}
