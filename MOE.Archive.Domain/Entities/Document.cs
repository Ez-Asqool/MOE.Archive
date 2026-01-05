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
    public class Document : BaseEntity
    {
        public Guid Id { get; set; }

        public string OriginalName { get; set; } = default!;

        public string FileName { get; set; } = default!;

        public string FilePath { get; set; } = default!;

        public string MimeType { get; set; } = default!;

        public long FileSize { get; set; }

        

        // Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = default!;

        // Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public OcrStatus OcrStatus { get; set; } = OcrStatus.Pending;

        public IndexStatus IndexStatus { get; set; } = IndexStatus.Pending;

        public string? Checksum { get; set; }

        public string? Summary { get; set; }

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
