using Microsoft.EntityFrameworkCore;
using MOE.Archive.Domain.Entities;
using MOE.Archive.Domain.Interfaces;
using MOE.Archive.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Category>> GetAllWithChildrenAsync(CancellationToken ct)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
        }

        public async Task<List<Category>> GetAllowedForDepartmentAsync(int departmentId, CancellationToken ct)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.DepartmentId == null || c.DepartmentId == departmentId)
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
        }
    }
}
