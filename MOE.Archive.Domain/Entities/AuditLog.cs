using MOE.Archive.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Nullable for actions not related to a specific document (e.g. Login).
        /// </summary>
        public Guid? DocumentId { get; set; }
        public Document? Document { get; set; }

        public AuditAction Action { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }

        public string? Details { get; set; }
    }
}
