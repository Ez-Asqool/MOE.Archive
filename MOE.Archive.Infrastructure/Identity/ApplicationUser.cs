using Microsoft.AspNetCore.Identity;
using MOE.Archive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = default!;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string JobNumber { get; set; }

        public bool IsActive { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
