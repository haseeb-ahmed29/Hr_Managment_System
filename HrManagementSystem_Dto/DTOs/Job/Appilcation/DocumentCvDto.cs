namespace HrMangmentSystem_Dto.DTOs.Job.Appilcation
{
    public class DocumentCvDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }

        public string CandidateName { get; set; } = null!;
        public string CandidateEmail { get; set; } = null!;
        public string? CandidatePhone { get; set; }

    }
}
