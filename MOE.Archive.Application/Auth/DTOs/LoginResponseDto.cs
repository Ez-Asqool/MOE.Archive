using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Auth.DTOs
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? JobNumber { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
