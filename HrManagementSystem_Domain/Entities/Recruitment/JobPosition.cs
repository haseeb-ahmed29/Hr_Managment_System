using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Employees;

namespace HrMangmentSystem_Domain.Entities.Recruitment
{
    public class JobPosition : SoftDeletable<int>
    {
  
        public string Title { get; set; }  = null!;
        public string? Description { get; set; } 
        public string Requirements { get; set; }  = null!;
        public DateTime PostedDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public bool IsActive { get; set; } 

        public int DepartmentId { get; set; } 
        public Department Department { get; set; } = null!;

        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    }
}
