using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Entities
{
    public class Department : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
