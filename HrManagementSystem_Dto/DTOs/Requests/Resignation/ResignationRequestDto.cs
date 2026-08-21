namespace HrMangmentSystem_Dto.DTOs.Requests.Resignation
{
    public class ResignationRequestDto
    {
        public int Id { get; set; }

        public DateTime ProposedLastWorkingDate { get; set; }

        public string Reason { get; set; } = null!;

        public bool IsVoluntary { get; set; }


        public string? Notes { get; set; }
    }
}
