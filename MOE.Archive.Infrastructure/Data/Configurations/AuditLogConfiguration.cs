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
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // Table
            builder.ToTable("AuditLogs");

            // Key
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.DocumentId)
                .IsRequired(false);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasConversion<int>(); // store enum as int

            builder.Property(a => a.Timestamp)
                .IsRequired();

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45); // IPv4 / IPv6 safe

            builder.Property(a => a.Details)
                .HasColumnType("nvarchar(max)");

            // Relationships

            // AuditLog ↔ Document (optional)
            builder.HasOne(a => a.Document)
                   .WithMany(d => d.AuditLogs)
                   .HasForeignKey(a => a.DocumentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
