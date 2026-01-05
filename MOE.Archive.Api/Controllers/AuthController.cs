using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MOE.Archive.Application.Auth.DTOs;
using MOE.Archive.Application.Auth.Services;

namespace MOE.Archive.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request, HttpContext.RequestAborted);

            if (!result.Success)
                return Unauthorized(new { message = result.ErrorMessage });

            return Ok(result);
        }
    }
}
