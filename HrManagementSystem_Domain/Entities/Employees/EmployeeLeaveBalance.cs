using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Employees
{

    public class EmployeeLeaveBalance : SoftDeletable<int>
    {
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public LeaveType LeaveType { get; set; }

    
        public decimal BalanceDays { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
