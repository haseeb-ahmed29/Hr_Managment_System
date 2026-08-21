using HrManagmentSystem_Shared.Enum.Employee;
using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Employees
{
    public class DocumentEmployeeInfo : SoftDeletable<Guid>
    {
        public Guid EmployeeId { get; set; } 
        public Employee Employee { get; set; } = null!;


        public DocumentType DocumentCategory { get; set; }

        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }


        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiryDate { get; set; } 

    }
}
 