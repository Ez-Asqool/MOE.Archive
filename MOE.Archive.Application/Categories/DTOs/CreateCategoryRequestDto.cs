using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Categories.DTOs
{
    public class CreateCategoryRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }

        // Admin can set this to create department-specific category
        public int? DepartmentId { get; set; }
    }
}
