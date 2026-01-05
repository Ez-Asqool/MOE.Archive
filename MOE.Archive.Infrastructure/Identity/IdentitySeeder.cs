using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Identity
{
    public class IdentitySeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentitySeeder(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            // 1) Roles
            await EnsureRoleAsync("Admin", "Administrator role with full permissions", cancellationToken);
            await EnsureRoleAsync("Employee", "Standard user role with limited permissions", cancellationToken);

            // 2) Admin user
            const string adminEmail = "admin@moe.ps";
            const string adminPassword = "MOEadmin@123*";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,

                    FullName = "System Admin",
                    JobNumber = "ADMIN-0001",
                    IsActive = true,
                    IsDeleted = false
                };

                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }
            }

            // Make sure admin has Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to add Admin role to admin user: {errors}");
                }
            }
        }

        private async Task EnsureRoleAsync(string roleName, string? description, CancellationToken ct)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null) return;

            role = new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                IsDeleted = false
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
            }
        }
    }
}
