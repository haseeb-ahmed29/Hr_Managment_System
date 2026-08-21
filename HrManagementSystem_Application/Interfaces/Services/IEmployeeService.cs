using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Employee;

namespace HrMangmentSystem_Application.Interfaces.Services
{
    public interface IEmployeeService 
    {
        Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);
        Task<ApiResponse<EmployeeDto?>> GetEmployeeByIdAsync(Guid employeeId);
        Task<ApiResponse<List<EmployeeDto>>> GetAllEmployeesAsync();
        Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(UpdateEmployeeDto updateEmployeeDto);
        Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId );

        Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesPagedAsync(PagedRequest request);

        Task<ApiResponse<bool>> AssignManagerAsync(Guid employeeId, Guid managerId);

    }
}
