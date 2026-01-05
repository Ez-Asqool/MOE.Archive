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
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            // Table
            builder.ToTable("Documents");

            // Key
            builder.HasKey(d => d.Id);

            BaseEntityConfiguration.ConfigureBaseEntity(builder);

            // Properties
            builder.Property(x => x.OriginalName)
            .IsRequired()
            .HasMaxLength(260);

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(360);

            builder.Property(d => d.FilePath)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(d => d.MimeType)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(d => d.FileSize)
                .IsRequired();

            builder.Property(d => d.CreatedBy)
                .IsRequired();

            builder.Property(d => d.DepartmentId)
                .IsRequired();

            builder.Property(d => d.CategoryId)
                .IsRequired();

            builder.Property(d => d.OcrStatus)
                .IsRequired()
                .HasConversion<int>();   // store enum as int

            builder.Property(d => d.IndexStatus)
                .IsRequired()
                .HasConversion<int>();   // store enum as int

            builder.Property(d => d.Checksum)
                .HasMaxLength(250);

            builder.Property(d => d.Summary)
                .HasColumnType("nvarchar(max)"); // or let EF choose default

            // Relationships

            // Document ↔ Department (many documents in one department)
            builder.HasOne(d => d.Department)
                   .WithMany(dep => dep.Documents)
                   .HasForeignKey(d => d.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Document ↔ Category (many documents in one category)
            builder.HasOne(d => d.Category)
                   .WithMany(cat => cat.Documents)
                   .HasForeignKey(d => d.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Document ↔ AuditLogs (one document, many logs)
            builder.HasMany(d => d.AuditLogs)
                   .WithOne(a => a.Document)
                   .HasForeignKey(a => a.DocumentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
