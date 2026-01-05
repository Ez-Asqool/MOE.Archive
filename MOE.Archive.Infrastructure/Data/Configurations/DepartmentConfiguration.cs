using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOE.Archive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            // Table
            builder.ToTable("Departments");

            // Key
            builder.HasKey(d => d.Id);

            BaseEntityConfiguration.ConfigureBaseEntity(builder);

            // Properties
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Code)
                .HasMaxLength(50);

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasMany(d => d.Documents)
                   .WithOne(doc => doc.Department)
                   .HasForeignKey(doc => doc.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
