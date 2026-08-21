using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Employees;

namespace HrMangmentSystem_Domain.Entities.Roles
{
    public class EmployeeRole : TenantEntity<int>
    {
        public Guid EmployeeId { get; set; } 
        public Employee Employee { get; set; } = null!;


        public int RoleId { get; set; } 
        public Role Role { get; set; } = null!;
    }
}
