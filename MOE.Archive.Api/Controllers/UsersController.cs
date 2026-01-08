using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.UserManagement.DTOs;
using MOE.Archive.Application.UserManagement.Services;
using System.Security.Claims;

namespace MOE.Archive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized();

            var result = await _userManagementService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized();

            var result = await _userManagementService.GetAllGroupedByDepartmentAsync(ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateUserRequestDto request, CancellationToken ct)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized();

            var adminId = Guid.Parse(adminIdStr);

            var result = await _userManagementService.CreateUserByAdminAsync(request, adminId, ct);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequestDto request, CancellationToken ct)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized();

            var result = await _userManagementService.UpdateAsync(request, Guid.Parse(adminIdStr), ct);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var adminIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminIdStr))
                return Unauthorized();

            var result = await _userManagementService.DeleteAsync(id, Guid.Parse(adminIdStr), ct);
            return Ok(result);
        }

        
    }
}
