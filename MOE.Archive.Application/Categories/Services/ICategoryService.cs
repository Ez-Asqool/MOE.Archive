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
        Task<CategoryResponseDto> GetByIdAsync(int id,  CancellationToken cancellationToken = default);

        Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, Guid? createdBy, CancellationToken cancellationToken = default);

        Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request, Guid? updatedBy, CancellationToken cancellationToken = default);

        Task<CategoryResponseDto> DeleteAsync(int id, Guid? deletedBy, CancellationToken cancellationToken = default);
    }
}
