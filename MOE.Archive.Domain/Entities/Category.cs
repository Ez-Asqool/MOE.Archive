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
    public class Category : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public bool IsActive { get; set; } = true;

       

        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
