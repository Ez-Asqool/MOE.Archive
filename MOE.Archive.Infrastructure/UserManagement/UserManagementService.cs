using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MOE.Archive.Application.UserManagement.DTOs;
using MOE.Archive.Application.UserManagement.Services;
using MOE.Archive.Domain.Interfaces;
using MOE.Archive.Infrastructure.Data;
using MOE.Archive.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.UserManagement
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IDepartmentRepository _departmentRepository;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IDepartmentRepository departmentRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _departmentRepository = departmentRepository;
        }

        public async Task<UserResponseDto> CreateUserByAdminAsync(
            AdminCreateUserRequestDto request,
            Guid adminUserId,
            CancellationToken ct = default)
        {
            // 1) Validate department exists
            var dept = await _departmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dept == null)
                throw new KeyNotFoundException("القسم غير موجود.");

            var role = request.Role.Trim();

            // 2) Ensure role exists
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
                throw new InvalidOperationException($"Role '{role}' is not seeded.");

            // 3) Unique email
            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail != null)
                throw new InvalidOperationException("البريد الإلكتروني مستخدم بالفعل.");

            // 4) Unique JobNumber
            var jobExists = await _userManager.Users
                .AnyAsync(u => u.JobNumber == request.JobNumber && !u.IsDeleted, ct);

            if (jobExists)
                throw new InvalidOperationException("الرقم الوظيفي مستخدم بالفعل.");

            // 5) Rule: only ONE DeptAdmin per Department
            if (role == "DeptAdmin")
            {
                var deptAdminRole = await _roleManager.FindByNameAsync("DeptAdmin");
                if (deptAdminRole == null)
                    throw new InvalidOperationException("Role 'DeptAdmin' is not seeded.");

                var deptAdminExists = await (from u in _context.Users
                                             join ur in _context.UserRoles on u.Id equals ur.UserId
                                             where u.DepartmentId == request.DepartmentId
                                                   && ur.RoleId == deptAdminRole.Id
                                                   && !u.IsDeleted
                                             select u.Id)
                                            .AnyAsync(ct);

                if (deptAdminExists)
                    throw new InvalidOperationException("يوجد مسؤول قسم (DeptAdmin) لهذا القسم بالفعل.");
            }

            // 6) Create user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,

                FullName = request.FullName,
                DepartmentId = request.DepartmentId,
                JobNumber = request.JobNumber,
                PhoneNumber = request.PhoneNumber,

                IsActive = true,
                IsDeleted = false,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUserId
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"فشل إنشاء المستخدم: {errors}");
            }

            // 7) Assign role
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"فشل تعيين الدور: {errors}");
            }

            //return new AdminCreateUserResponseDto
            //{
            //    Id = user.Id,
            //    FullName = user.FullName,
            //    Email = user.Email!,
            //    Role = role,
            //    DepartmentId = user.DepartmentId ?? 0,
            //    IsActive = user.IsActive
            //};
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                JobNumber = user.JobNumber,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Role = role
            };
        }

        public async Task<UserResponseDto> UpdateAsync(UpdateUserRequestDto request, Guid adminUserId, CancellationToken ct = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == request.Id, ct);
            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            // Update basic fields (only if provided)
            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber.Trim();

            if (request.JobNumber != null)
                user.JobNumber = request.JobNumber.Trim();

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            if (request.DepartmentId.HasValue)
                user.DepartmentId = request.DepartmentId.Value;

            // Email update (also update UserName since you login by email)
            if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var newEmail = request.Email.Trim();

                // Ensure unique email
                var exists = await _userManager.FindByEmailAsync(newEmail);
                if (exists != null && exists.Id != user.Id)
                    throw new InvalidOperationException("البريد الإلكتروني مستخدم بالفعل.");

                user.Email = newEmail;
                user.NormalizedEmail = newEmail.ToUpperInvariant();

                user.UserName = newEmail;
                user.NormalizedUserName = newEmail.ToUpperInvariant();
            }

            // Role update (if provided)
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var newRole = request.Role.Trim();

                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(newRole))
                    throw new InvalidOperationException("الدور المطلوب غير موجود.");

                // Enforce one DeptAdmin per department
                if (newRole == "DeptAdmin")
                {
                    if (!user.DepartmentId.HasValue)
                        throw new InvalidOperationException("لا يمكن تعيين DeptAdmin بدون تحديد القسم.");

                    var departmentId = user.DepartmentId.Value;

                    // anyone else in this dept already DeptAdmin?
                    var deptAdmins = await _context.Users
                        .Where(u => u.DepartmentId == departmentId && u.Id != user.Id)
                        .ToListAsync(ct);

                    foreach (var u in deptAdmins)
                    {
                        if (await _userManager.IsInRoleAsync(u, "DeptAdmin"))
                            throw new InvalidOperationException("يوجد بالفعل مدير قسم (DeptAdmin) لهذا القسم.");
                    }
                }

                // Replace roles (keep it simple: user must be either Employee or DeptAdmin)
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(newRole))
                {
                    if (currentRoles.Count > 0)
                    {
                        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                        if (!removeResult.Succeeded)
                            throw new InvalidOperationException(string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }

                    var addResult = await _userManager.AddToRoleAsync(user, newRole);
                    if (!addResult.Succeeded)
                        throw new InvalidOperationException(string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }
            }

            user.UpdatedBy = adminUserId;   
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

            // Return response
            var roles = await _userManager.GetRolesAsync(user);
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                JobNumber = user.JobNumber,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? ""
            };
        }

        public async Task<UserResponseDto> DeleteAsync(Guid id, Guid? deletedBy, CancellationToken ct = default)
        {
            if (deletedBy is null)
                throw new UnauthorizedAccessException("غير مصرح.");

            // Important: if you have a global query filter on users (IsDeleted == false),
            // this will not find already-deleted users (which is good).
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            // Optional safety: prevent deleting the super admin by email (if you want)
             if (string.Equals(user.Email, "admin@moe.ps", StringComparison.OrdinalIgnoreCase))
                 throw new InvalidOperationException("لا يمكن حذف حساب المدير الرئيسي.");

            // Soft delete
            user.IsDeleted = true;
            user.IsActive = false;

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = deletedBy;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

            // Return response (role is still readable even if user is soft-deleted)
            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                JobNumber = user.JobNumber,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? ""
            };
        }

        public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // Because of soft-delete query filter (if exists),
            // this will NOT return deleted users.
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, ct);

            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                JobNumber = user.JobNumber,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<List<DepartmentUsersGroupDto>> GetAllGroupedByDepartmentAsync(CancellationToken ct = default)
        {
            // Users (soft delete filter applies if configured on ApplicationUser)
            var usersQuery = _userManager.Users.AsNoTracking();

            // Build (UserId -> RoleName) map in ONE query
            var userRoles = await (
                from ur in _context.Set<IdentityUserRole<Guid>>().AsNoTracking()
                join r in _context.Set<ApplicationRole>().AsNoTracking() on ur.RoleId equals r.Id
                select new
                {
                    ur.UserId,
                    RoleName = r.Name
                }
            ).ToListAsync(ct);

            // Convert to dictionary (take first role if multiple)
            var roleByUserId = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).FirstOrDefault() ?? string.Empty);

            // Departments list (to show department name even if no users)
            var departments = await _context.Departments
                .AsNoTracking()
                .Select(d => new { d.Id, d.Name })
                .ToListAsync(ct);

            // Load users
            var users = await usersQuery
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.JobNumber,
                    u.DepartmentId,
                    u.IsActive
                })
                .ToListAsync(ct);

            // Group users by DepartmentId
            var grouped = users
                .GroupBy(u => u.DepartmentId)
                .Select(g =>
                {
                    var deptName = g.Key == null
                        ? "بدون قسم"
                        : departments.FirstOrDefault(d => d.Id == g.Key)?.Name ?? "قسم غير معروف";

                    return new DepartmentUsersGroupDto
                    {
                        DepartmentId = g.Key,
                        DepartmentName = deptName,
                        Users = g.Select(u => new UserResponseDto
                        {
                            Id = u.Id,
                            FullName = u.FullName,
                            Email = u.Email ?? string.Empty,
                            PhoneNumber = u.PhoneNumber,
                            JobNumber = u.JobNumber,
                            DepartmentId = u.DepartmentId,
                            IsActive = u.IsActive,
                            Role = roleByUserId.TryGetValue(u.Id, out var role) ? role : string.Empty
                        })
                        .OrderBy(x => x.FullName)
                        .ToList()
                    };
                })
                .OrderBy(x => x.DepartmentId == null ? -1 : x.DepartmentId) // "No Dept" first
                .ToList();

            return grouped;
        }
    }
    
}
