using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.DTOs
{
    public class UpdateUserRequestDto
    {
        public Guid Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? JobNumber { get; set; }

        public int? DepartmentId { get; set; }

        public bool? IsActive { get; set; }

        // Allowed: "Employee", "DeptAdmin"
        public string? Role { get; set; }
    }
}
