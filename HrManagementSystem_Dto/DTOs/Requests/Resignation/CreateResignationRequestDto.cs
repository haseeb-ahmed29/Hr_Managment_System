namespace HrMangmentSystem_Dto.DTOs.Requests.Resignation
{
    public class CreateResignationRequestDto
    {
        public DateTime ProposedLastWorkingDate { get; set; }

        public string Reason { get; set; } = null!;

        public bool IsVoluntary { get; set; } = true;


        public string? Notes { get; set; }
    }
}
