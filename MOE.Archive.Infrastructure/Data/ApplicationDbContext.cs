using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MOE.Archive.Domain.Entities;
using MOE.Archive.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Archive domain tables
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AppUser soft delete filter
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => !u.IsDeleted);

            // AppRole soft delete filter
            builder.Entity<ApplicationRole>()
                .HasQueryFilter(r => !r.IsDeleted);


            // Apply all IEntityTypeConfiguration<> in this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // ===============================
            // Document.CreatedBy (REQUIRED FK)
            // ===============================
            builder.Entity<Document>()
                .HasOne<ApplicationUser>()                     // principal type
                .WithMany(u => u.UploadedDocuments)            // navigation on user
                .HasForeignKey(d => d.CreatedBy)        // FK property on Document
                .OnDelete(DeleteBehavior.Restrict);            // don't cascade delete users

            // ===============================
            // AuditLog.UserId (REQUIRED FK)
            // ===============================
            builder.Entity<AuditLog>()
                .HasOne<ApplicationUser>()                     // principal type
                .WithMany(u => u.AuditLogs)                    // navigation on user
                .HasForeignKey(a => a.UserId)                  // FK property on AuditLog
                .OnDelete(DeleteBehavior.Restrict);



            // Here you can add Fluent API configs if needed
            // Example: soft delete query filters can be added later
            // 3) Global soft-delete filter for all entities that inherit BaseEntity
            ApplySoftDeleteFilters(builder);



        }

        private static void ApplySoftDeleteFilters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(BaseEntity).IsAssignableFrom(clrType))
                {
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter),
                            BindingFlags.NonPublic | BindingFlags.Static)?
                        .MakeGenericMethod(clrType);

                    method?.Invoke(null, new object[] { builder });
                }
            }
        }

        // Generic helper used via reflection above
        private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
            where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
