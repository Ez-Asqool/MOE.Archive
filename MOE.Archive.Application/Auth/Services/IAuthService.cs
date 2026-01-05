using MOE.Archive.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Auth.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync( LoginRequestDto loginDto, CancellationToken cancellationToken = default);

        //for scenario where you’ll need credential checks without login:
        Task<bool> ValidateUserAsync(string userNameOrEmail, string password, CancellationToken cancellationToken = default);
    }
}
