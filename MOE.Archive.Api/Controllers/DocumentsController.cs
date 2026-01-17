using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.Documents.DTOs;
using MOE.Archive.Application.Documents.Services;
using System.Security.Claims;

namespace MOE.Archive.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequestDto request, CancellationToken ct)
        {
            // UserId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            // Role
            var isAdmin = User.IsInRole("Admin");

            // DepartmentId from JWT claim (for Employee/DeptAdmin)
            int? callerDeptId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                callerDeptId = depId;

            var result = await _documentService.UploadAsync(request, userId, isAdmin, callerDeptId, ct);
            return Ok(result);
        }

        //[HttpPost("upload")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> Upload([FromForm] UploadDocumentRequestDto request, CancellationToken ct)
        //{
        //    var userId = GetUserId();

        //    var result = await _documentService.UploadAsync(request, userId, ct);
        //    return Ok(result);
        //}

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequestDto request, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

            // If you have DepartmentId in JWT claims (recommended):
            int? deptId = null;
            var deptClaim = User.FindFirstValue("DepartmentId");
            if (int.TryParse(deptClaim, out var parsedDept))
                deptId = parsedDept;

            var result = await _documentService.UpdateAsync(id, request, userId, role, deptId, ct);
            return Ok(result);
        }


        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
