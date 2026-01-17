using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.DTOs
{
    public class UpdateDocumentRequestDto
    {
        public string? OriginalName { get; set; }
        public int? CategoryId { get; set; }
    }
}
