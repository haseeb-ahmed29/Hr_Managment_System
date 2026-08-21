using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Employees
{
    public class Department : SoftDeletable<int>
    {
        public int? ParentDepartmentId { get; set; } 
        public string Code { get; set; } = null!;

        public string DeptName { get; set; } = null!;

        public string? Description { get; set; }

        public string Location { get; set; } = null!;

        
        public Guid? DepartmentManagerId { get; set; }
        public Employee? DepartmentManager { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
       
    }
}
