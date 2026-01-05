using MOE.Archive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Interfaces
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Document>> SearchAsync(string? searchTerm, CancellationToken cancellationToken = default);

    }
}
