using MOE.Archive.Application.UserManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.Services
{
    public interface IUserManagementService
    {
        Task<UserResponseDto> CreateUserByAdminAsync(AdminCreateUserRequestDto request, Guid adminUserId, CancellationToken ct = default);

        Task<UserResponseDto> UpdateAsync(UpdateUserRequestDto request, Guid adminUserId, CancellationToken ct = default);

        Task<UserResponseDto> DeleteAsync(Guid id, Guid? deletedBy, CancellationToken ct = default);

        Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<List<DepartmentUsersGroupDto>> GetAllGroupedByDepartmentAsync(CancellationToken ct = default);

        Task<List<UserResponseDto>> GetMyDepartmentUsersAsync(int departmentId, CancellationToken ct = default);


    }
}
