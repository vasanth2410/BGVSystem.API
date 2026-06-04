using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public string ToEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? SentAt { get; set; }

        public int RetryCount { get; set; }

        public int MaxRetryCount { get; set; } = 3;

        public DateTime? LastAttemptAt { get; set; }

        public string? ErrorMessage { get; set; }

        
    }
}
