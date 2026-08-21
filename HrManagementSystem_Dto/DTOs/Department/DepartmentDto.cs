namespace HrMangmentSystem_Dto.DTOs.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
    }
}
