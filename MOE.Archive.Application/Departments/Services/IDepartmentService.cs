using MOE.Archive.Application.Departments.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Departments.Services
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DepartmentResponseDto> CreateAsync(CreateDepartmentRequestDto requestDto, Guid? userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DepartmentResponseDto> UpdateAsync(int id, UpdateDepartmentRequestDto requestDto, Guid? userId, CancellationToken cancellationToken = default);
        Task<DepartmentResponseDto> DeleteAsync(int id, Guid? userId, CancellationToken cancellationToken = default);
    }
}
