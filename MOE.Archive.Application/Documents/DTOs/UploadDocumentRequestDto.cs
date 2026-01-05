using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.DTOs
{
    public class UploadDocumentRequestDto
    {
        public int CategoryId { get; set; }
        public int DepartmentId { get; set; }
        public IFormFile File { get; set; } = default!;
    }
}
