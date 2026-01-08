using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.DTOs
{
    public class DepartmentUsersGroupDto
    {
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public List<UserResponseDto> Users { get; set; } = new();
    }
}
