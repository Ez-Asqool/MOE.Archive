using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Archive.DTOs
{
    public class ArchiveTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
        public int? DepartmentId { get; set; }

        public List<ArchiveTreeDto> Children { get; set; } = new();
        public List<DocumentListItemDto> Documents { get; set; } = new();
    }
}
