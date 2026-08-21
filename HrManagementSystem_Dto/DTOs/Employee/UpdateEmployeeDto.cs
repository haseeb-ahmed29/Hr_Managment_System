using System.ComponentModel.DataAnnotations;

namespace HrMangmentSystem_Dto.DTOs.Employee
{
    public class UpdateEmployeeDto
    {
        public Guid Id { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
        public int? DepartmentId { get; set; }
        public string? Position { get; set; }
        public string? Address { get; set; }
        
    }
}
