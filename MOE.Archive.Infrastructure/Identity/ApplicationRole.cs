using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Identity
{
    // Identity role with Guid key (to match ApplicationUser)
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        public ApplicationRole()
        {
            IsDeleted = false;
        }
    }
}
