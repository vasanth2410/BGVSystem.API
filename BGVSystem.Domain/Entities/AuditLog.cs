using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string Action { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }
    }
}
