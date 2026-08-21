using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Roles
{
    public class Role : TenantEntity<int>
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    }
}
