using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Domain.Common;

namespace HrMangmentSystem_Domain.Entities.Requests
{
    public class FinancialRequest : SoftDeletable<int>
    {
        
        public int GenericRequestId { get; set; }
        public GenericRequest GenericRequest { get; set; } = null!;

        public FinancialType FinancialType { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "JOD";

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? Details { get; set; }
    }
}
