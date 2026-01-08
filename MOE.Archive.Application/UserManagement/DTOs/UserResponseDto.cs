using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? JobNumber { get; set; }

        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
