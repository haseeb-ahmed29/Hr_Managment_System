using HrManagmentSystem_Shared.Enum.Employee;
using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Roles;

namespace HrMangmentSystem_Domain.Entities.Employees
{
    public class Employee : SoftDeletable<Guid>
    {
        public Guid? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        public int DepartmentId { get; set; } 
        public Department Department { get; set; } = null!;


        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


        public Gender Gender { get; set; }

        public string PasswordHash { get; set; } = null!;
        public bool MustChangePassword { get; set; }
        public DateTime? LastPasswordChangeAt { get; set; }

        public string Position { get; set; } = null!;
        public string Address { get; set; } = null!;

        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }


        public EmployeeStatus EmploymentStatusType { get; set; }

        public ICollection<Employee> Subordinates { get; set; } = new List<Employee>(); //employees managed by this employee

        public ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

    }
}
