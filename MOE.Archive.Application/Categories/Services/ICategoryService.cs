using MOE.Archive.Application.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Categories.Services
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> GetByIdAsync(int id, bool isAdmin, int? callerDepartmentId, CancellationToken cancellationToken = default);

        Task<List<CategoryResponseDto>> GetAllAsync(bool isAdmin, int? callerDepartmentId, CancellationToken ct = default);
        
        Task<List<CategoryResponseDto>> GetMainCategoriesAsync(bool isAdmin, CancellationToken ct = default);

        Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, Guid? createdBy, bool isAdmin, bool isDeptAdmin, int? callerDepartmentId, CancellationToken cancellationToken = default);

        Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request, Guid? updatedBy, bool isAdmin, bool isDeptAdmin, int? callerDepartmentId, CancellationToken cancellationToken = default);

        Task<CategoryResponseDto> DeleteAsync(int id, Guid? deletedBy, bool isAdmin, bool isDeptAdmin, int? callerDepartmentId, CancellationToken cancellationToken = default);
    }
}
