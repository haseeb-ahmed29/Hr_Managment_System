namespace HrMangmentSystem_Dto.DTOs.Employee
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string Position { get; set; } = null!;

        public DateTime EmploymentStartDate { get; set; }
        public DateTime EmploymentEndDate { get; set; }




    }
}
