using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Common.Storage
{
    public interface IFileStorage
    {
        Task<(string relativePath, string savedFileName)> SaveAsync(
            Stream fileStream,
            string originalFileName,
            int departmentId,
            int categoryId,
            Guid documentId,
            CancellationToken ct = default);
    }
}
