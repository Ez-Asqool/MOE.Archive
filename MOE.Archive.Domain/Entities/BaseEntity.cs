using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Entities
{
    public abstract class BaseEntity
    {
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User Id of the creator (optional, can be filled from current user).
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// User Id of the last modifier (optional).
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
