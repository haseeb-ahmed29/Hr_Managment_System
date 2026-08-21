using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface ILeaveBalanceService
    {

        Task<ApiResponse<List<EmployeeLeaveBalanceDto>>> GetMyLeaveBalanceAsync();

        Task<bool> TryConsumeLeaveAsync(Guid employeeId, LeaveType leaveType, decimal days);

        //Return days to employee balance after cancle request 
        Task RefundLeaveAsync(Guid employeeId, LeaveType leaveType, decimal days);

        Task InitializeAnnualLeaveForEmployeeAsync(Guid employeeId, DateTime employmentStartDate);

        Task<bool> HasSufficientBalanceAsync(Guid employeeId, LeaveType leaveType, decimal days);
    }
}
