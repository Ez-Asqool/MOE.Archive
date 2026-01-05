using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOE.Archive.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Data.Configurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("AspNetRoles");

            builder.Property(r => r.Description)
                   .HasMaxLength(250);

            // Audit defaults handled by SQL Server (NO UtcNow in C#)
            builder.Property(r => r.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.IsDeleted)
                   .HasDefaultValue(false);

            // Global Soft Delete
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
