using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.Categories.DTOs;
using MOE.Archive.Application.Categories.Services;
using System.Security.Claims;

namespace MOE.Archive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,DeptAdmin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
                _categoryService = categoryService;
        }

        //[HttpGet("{id}")]   
        //public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        //{
        //    var categoryResponseDto = await _categoryService.GetByIdAsync(id, cancellationToken);
        //    return Ok(categoryResponseDto);
        //}

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            bool isAdmin = User.IsInRole("Admin");

            int? departmentId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                departmentId = depId;

            var result = await _categoryService.GetByIdAsync(id, isAdmin, departmentId, ct);
            return Ok(result);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        //{
        //    var categoryResponseDtos = await _categoryService.GetAllAsync(cancellationToken);
        //    return Ok(categoryResponseDtos);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            bool isAdmin = User.IsInRole("Admin");

            int? departmentId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                departmentId = depId;

            var result = await _categoryService.GetAllAsync(isAdmin, departmentId, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request, CancellationToken ct)
        {
            // UserId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            // Roles
            var isAdmin = User.IsInRole("Admin");
            var isDeptAdmin = User.IsInRole("DeptAdmin");

            // DepartmentId (from token claim)
            int? departmentId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                departmentId = depId;

            var result = await _categoryService.CreateAsync(
                request,
                createdBy: userId,
                isAdmin: isAdmin,
                isDeptAdmin: isDeptAdmin,
                callerDepartmentId: departmentId,
                cancellationToken: ct);

            return Ok(result);
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto createCategoryRequestDto, CancellationToken cancellationToken)
        //{
        //    if(createCategoryRequestDto == null)
        //        return BadRequest();

        //    var userId = GetUserId();

        //    var isAdmin = User.IsInRole("Admin");   
        //    var isDeptAdmin = User.IsInRole("DeptAdmin");   

        //    var categoryResponseDto = await _categoryService.CreateAsync(createCategoryRequestDto, userId, isAdmin, isDeptAdmin, cancellationToken);
        //    return CreatedAtAction(nameof(GetById), new { id = categoryResponseDto.Id }, categoryResponseDto);
        //}


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto request, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = Guid.Parse(userIdClaim.Value);

            bool isAdmin = User.IsInRole("Admin");
            bool isDeptAdmin = User.IsInRole("DeptAdmin");

            int? deptId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                deptId = depId;

            var result = await _categoryService.UpdateAsync(id, request, userId, isAdmin, isDeptAdmin, deptId, ct);
            return Ok(result);
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto updateCategoryRequestDto, CancellationToken cancellationToken)
        //{
        //    if(updateCategoryRequestDto == null)
        //        return BadRequest();

        //    var userId = GetUserId();

        //    var categoryResponseDto = await _categoryService.UpdateAsync(id, updateCategoryRequestDto, userId, cancellationToken);
        //    return Ok(categoryResponseDto);
        //}


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);

            bool isAdmin = User.IsInRole("Admin");
            bool isDeptAdmin = User.IsInRole("DeptAdmin");

            int? deptId = null;
            var deptClaim = User.FindFirst("DepartmentId");
            if (deptClaim != null && int.TryParse(deptClaim.Value, out var depId))
                deptId = depId;

            var result = await _categoryService.DeleteAsync(id, userId, isAdmin, isDeptAdmin, deptId, ct);
            return Ok(result);
        }
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        //{
        //    var userId = GetUserId();

        //    var categoryResponseDto = await _categoryService.DeleteAsync(id, userId, cancellationToken);  
        //    return Ok(categoryResponseDto);
        //}


        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
