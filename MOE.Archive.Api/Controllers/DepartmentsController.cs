using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.Departments.DTOs;
using MOE.Archive.Application.Departments.Services;
using System.Security.Claims;

namespace MOE.Archive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var departmentResponseDtos = await _departmentService.GetAllAsync(cancellationToken); 
            return Ok(departmentResponseDtos);  
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var departmentResponseDto = await _departmentService.GetByIdAsync(id, cancellationToken);
            return Ok(departmentResponseDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequestDto requestDto, CancellationToken cancellationToken) 
        {
            if (requestDto == null)
                return BadRequest();

            var userId = GetUserId();
            
            var departmentResponseDto = await _departmentService.CreateAsync(requestDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = departmentResponseDto.Id}, departmentResponseDto);   
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequestDto requestDto, CancellationToken cancellationToken)
        {
            if (requestDto == null)
                return BadRequest();

            var userId = GetUserId();   

            var departmentResponseDto = await _departmentService.UpdateAsync(id, requestDto, userId, cancellationToken);  
            return Ok(departmentResponseDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var departmentResponseDto = await _departmentService.DeleteAsync(id, userId, cancellationToken);
            return Ok(departmentResponseDto);
        }



        private Guid? GetUserId() 
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;  
        }
    }
}
