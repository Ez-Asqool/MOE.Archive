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
    [Authorize(Roles = "Admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
                _categoryService = categoryService;
        }

        [HttpGet("{id}")]   
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var categoryResponseDto = await _categoryService.GetByIdAsync(id, cancellationToken);
            return Ok(categoryResponseDto);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categoryResponseDtos = await _categoryService.GetAllAsync(cancellationToken);
            return Ok(categoryResponseDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto createCategoryRequestDto, CancellationToken cancellationToken)
        {
            if(createCategoryRequestDto == null)
                return BadRequest();

            var userId = GetUserId();

            var categoryResponseDto = await _categoryService.CreateAsync(createCategoryRequestDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = categoryResponseDto.Id }, categoryResponseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto updateCategoryRequestDto, CancellationToken cancellationToken)
        {
            if(updateCategoryRequestDto == null)
                return BadRequest();

            var userId = GetUserId();

            var categoryResponseDto = await _categoryService.UpdateAsync(id, updateCategoryRequestDto, userId, cancellationToken);
            return Ok(categoryResponseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var categoryResponseDto = await _categoryService.DeleteAsync(id, userId, cancellationToken);  
            return Ok(categoryResponseDto);
        }


        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
