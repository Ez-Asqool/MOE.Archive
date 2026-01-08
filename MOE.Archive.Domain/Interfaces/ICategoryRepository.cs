using MOE.Archive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetAllWithChildrenAsync(CancellationToken ct);
        Task<List<Category>> GetAllowedForDepartmentAsync(int departmentId, CancellationToken ct);

    }
}
