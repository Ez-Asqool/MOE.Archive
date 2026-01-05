using Microsoft.AspNetCore.Identity;
using MOE.Archive.Application.Auth.DTOs;
using MOE.Archive.Application.Auth.Services;
using MOE.Archive.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponseDto> LoginAsync(
            LoginRequestDto loginDto,
            CancellationToken cancellationToken = default)
        {
            var response = new LoginResponseDto();

            // 1) Find user by email or username
            ApplicationUser? user =
                await _userManager.FindByEmailAsync(loginDto.UserNameOrEmail)
                ?? await _userManager.FindByNameAsync(loginDto.UserNameOrEmail);

            if (user == null || user.IsDeleted || !user.IsActive)
            {
                response.Success = false;
                response.ErrorMessage = "Invalid credentials.";
                return response;
            }

            // 2) Check password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                response.Success = false;
                response.ErrorMessage = "Invalid credentials.";
                return response;
            }

            // 3) Get roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4) Generate JWT
            var token = _jwtTokenService.GenerateToken(user, roles);
            var expiresAt = _jwtTokenService.GetExpiryTime();

            // 5) Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // 6) Fill response DTO
            response.Success = true;
            response.Token = token;
            response.ExpiresAt = expiresAt;
            response.UserId = user.Id.ToString();
            response.FullName = user.FullName;
            response.Email = user.Email;
            response.JobNumber = user.JobNumber;
            response.Roles = roles;

            return response;
        }


        //for scenario where you’ll need credential checks without login:
        public async Task<bool> ValidateUserAsync(
            string userNameOrEmail,
            string password,
            CancellationToken cancellationToken = default)
        {
            ApplicationUser? user =
                await _userManager.FindByEmailAsync(userNameOrEmail)
                ?? await _userManager.FindByNameAsync(userNameOrEmail);

            if (user == null || user.IsDeleted || !user.IsActive)
                return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}
