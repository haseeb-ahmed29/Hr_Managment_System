using HrManagmentSystem_Shared.Enum.Recruitment;

namespace HrMangmentSystem_Dto.DTOs.Job.Appilcation
{
    public class JobApplicationDto
    {
        public int Id { get; set; }

        public int DocumentCvId { get; set; }

        public int JobPositionId { get; set; }
        public string JobPositionTitle { get; set; } = null!;


        public JobApplicationStatus Status { get; set; }

        public DateTime AppliedAt { get; set; }

        public Guid? ReviewedByEmployeeId { get; set; }
        public string? ReviewedByEmployeeName { get; set; }

        public string? Notes { get; set; }
        public double? MatchScore { get; set; } // For Ai 

    }
}
