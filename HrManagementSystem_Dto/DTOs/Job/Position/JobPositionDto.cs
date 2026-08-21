namespace HrMangmentSystem_Dto.DTOs.Job.Position
{
    public class JobPositionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } 

        public string Requirements { get; set; } = null!;

        public bool IsActive { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = null!;
    }
}
