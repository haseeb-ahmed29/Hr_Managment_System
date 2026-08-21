using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Requests
{
    public class ResignationRequest : SoftDeletable<int>
    {
        public int GenericRequestId { get; set; }
        public GenericRequest GenericRequest { get; set; } = null!;

        public DateTime ProposdLastWorkWorkingDate { get; set; }

        public string Reason { get; set; } = null!;

        public bool IsVoluntary { get; set; }

        public string? Notes { get; set; }
    }
}
