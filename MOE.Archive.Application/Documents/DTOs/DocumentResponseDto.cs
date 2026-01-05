using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.DTOs
{
    public class DocumentResponseDto
    {
        public Guid Id { get; set; }

        public string OriginalName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public int DepartmentId { get; set; }
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
