using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.Archive.Services;

namespace MOE.Archive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArchiveController : ControllerBase
    {
        private readonly IArchiveService _archiveService;

        public ArchiveController(IArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        // Admin:  GET /api/archive?departmentId=3
        // Others: GET /api/archive  (uses claim DepartmentId)
        [HttpGet]
        public async Task<IActionResult> GetArchive([FromQuery] int? departmentId, CancellationToken ct)
        {
            bool isAdmin = User.IsInRole("Admin");

            // DepartmentId from JWT claim
            int? callerDeptId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                callerDeptId = depId;

            // Optional: if non-admin tries to pass departmentId, reject (more secure)
            if (!isAdmin && departmentId.HasValue)
                return Forbid();

            var result = await _archiveService.GetArchiveAsync(
                isAdmin: isAdmin,
                callerDepartmentId: callerDeptId,
                requestedDepartmentId: departmentId,
                ct: ct);

            return Ok(result);
        }
    }
}
