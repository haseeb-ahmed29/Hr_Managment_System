using HrMangmentSystem_Application.Common.Responses;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IEmployeeRoleService 
    {
        Task<ApiResponse<bool>> AssignDefaultRoleToEmployeeAsync(Guid employeId);
        Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(Guid employeeId , string roleName);
        Task<ApiResponse<bool>> UpdateRoleToEmployeeAsync(Guid employeeId , string roleName);

    }
}
