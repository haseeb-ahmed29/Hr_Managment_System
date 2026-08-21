using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.EmployeeData
{
    public class EmployeeLeaveBalanceDto
    {
        public LeaveType LeaveType { get; set; }
        public decimal BalanceDays { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
