using HrManagmentSystem_Shared.Enum.Recruitment;
using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Employees;

namespace HrMangmentSystem_Domain.Entities.Recruitment
{
    public class JobApplication : SoftDeletable<int>
    {
        public int JobPositionId { get; set; } 
        public JobPosition JobPosition { get; set; } = null!;

        public int DocumentCvId { get; set; } 
        public DocumentCv DocumentCv { get; set; } = null!;

        public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;


        public DateTime AppliedAt { get; set; } = DateTime.Now;

        public Guid? ReviewedByEmployeeId { get; set; }
        public Employee? ReviewedByEmployee { get; set; }

        public double? MatchScore { get; set; }
        public string? Notes { get; set; }

        }
}
