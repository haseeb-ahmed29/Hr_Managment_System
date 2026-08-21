using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Recruitment
{

    public class DocumentCv : TenantEntity<int>
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public string CandidateName { get; set; } = null!;
        public string CandidateEmail { get; set; } = null!;
        public string? CandidatePhone { get; set; }
    }



}
