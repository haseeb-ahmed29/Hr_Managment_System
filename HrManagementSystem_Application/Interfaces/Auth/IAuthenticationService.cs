using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Login;

namespace HrMangmentSystem_Application.Interfaces.Auth
{
    public interface IAuthenticationService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto);

        Task<ApiResponse<bool>> ChangePasswordAsync(Guid employeeId, ChangePasswordDto changePasswordDto);
    }
}
