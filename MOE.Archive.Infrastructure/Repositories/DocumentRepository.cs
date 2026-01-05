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
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Document>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => string.IsNullOrEmpty(searchTerm) ||
                            d.OriginalName.Contains(searchTerm) ||
                            d.FileName.Contains(searchTerm) ||
                            d.Summary!.Contains(searchTerm))
                .ToListAsync(cancellationToken);
        }
    }
}
