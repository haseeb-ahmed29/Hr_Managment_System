using HrManagmentSystem_Shared.Enum.Recruitment;

namespace HrMangmentSystem_Dto.DTOs.Job.Appilcation
{
    public class ChangeJobApplicationStatusDto
    {
        public int JobApplicationId { get; set; }

        public JobApplicationStatus Newstatus { get; set; }

        public string? Notes { get; set; }

    }
}
