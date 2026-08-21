namespace HrMangmentSystem_Dto.DTOs.Employee
{
    public class UpdateEmployeeRoleDto
    {
        public Guid EmployeeId { get; set; }
        public string RoleName { get; set; } = default!;
    }
}
