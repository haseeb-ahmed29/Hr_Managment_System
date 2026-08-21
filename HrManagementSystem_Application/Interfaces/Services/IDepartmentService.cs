using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Department;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IDepartmentService
    {
       Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);

        Task<ApiResponse<DepartmentDto?>> GetDepartmentByIdAsync(int departmentId);

       

        Task<ApiResponse<PagedResult<DepartmentDto>>> GetDepartmentPagedAsync(PagedRequest request);

        Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(UpdateDepartmentDto updateDepartmentDto);

        Task<ApiResponse<bool>> DeleteDepartmentAsync(int departmentId);

        Task<ApiResponse<bool>> SetParentDepartmentAsync(int departmentId, int? parentDepartmentId);
        Task<ApiResponse<bool>> SetDepartmentManagerAsync(int departmentId, Guid managerEmployeeId);


    }
}
