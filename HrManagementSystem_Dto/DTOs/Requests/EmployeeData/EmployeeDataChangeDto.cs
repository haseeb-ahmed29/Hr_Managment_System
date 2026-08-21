namespace HrMangmentSystem_Dto.DTOs.Requests.EmployeeData
{
    public class EmployeeDataChangeDto
    {
        public int Id { get; set; }

        public List<EmployeeDataChangeFieldDto> RequestedChanges { get; set; } = new();

        public List<EmployeeDataChangeFieldDto>? ApprovedChanges { get; set; }
        
        public DateTime? AppliedAt { get; set; }
    }
}
