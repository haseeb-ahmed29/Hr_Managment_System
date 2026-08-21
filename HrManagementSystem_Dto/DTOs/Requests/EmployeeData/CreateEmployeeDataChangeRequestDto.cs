namespace HrMangmentSystem_Dto.DTOs.Requests.EmployeeData
{
    public class CreateEmployeeDataChangeRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public List<EmployeeDataChangeFieldDto> Changes = new();
    }
    public class EmployeeDataChangeFieldDto
    {
        public string FieldName { get; set; } = null!;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
