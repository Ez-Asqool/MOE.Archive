using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Archive.DTOs
{
    public class DocumentListItemDto
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
