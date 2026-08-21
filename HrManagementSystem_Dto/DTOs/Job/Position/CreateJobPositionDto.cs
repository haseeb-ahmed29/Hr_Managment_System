namespace HrMangmentSystem_Dto.DTOs.Job.Position
{
    public class CreateJobPositionDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; } 

        public string Requirements { get; set; } = null!;

       
        public DateTime? ClosingDate { get; set; }
        public int DepartmentId { get; set; }
    }
}
