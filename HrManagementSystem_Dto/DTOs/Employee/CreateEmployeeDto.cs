using HrManagmentSystem_Shared.Enum.Employee;
using System.ComponentModel.DataAnnotations;

namespace HrMangmentSystem_Dto.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;

        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public int DepartmentId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Position { get; set; } = null!;
        public Gender Gender { get; set; }
        public EmployeeStatus Status { get; set; }

        public DateTime EmploymentStartDate { get; set; }
        public string Address { get; set; } = null!;

      

    }
}
