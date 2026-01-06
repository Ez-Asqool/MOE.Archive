using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Departments.DTOs
{
    public class DepartmentResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Code { get; set; }

        public bool IsActive { get; set; }
    }
}
