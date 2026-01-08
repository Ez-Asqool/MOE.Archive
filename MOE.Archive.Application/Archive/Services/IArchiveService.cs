using MOE.Archive.Application.Archive.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Archive.Services
{
    public interface IArchiveService
    {
        Task<List<ArchiveTreeDto>> GetArchiveAsync(
            bool isAdmin,
            int? callerDepartmentId,
            int? requestedDepartmentId,
            CancellationToken ct = default);
    }
}
