using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.Financial
{
    public class FinancialRequestDto
    {
        public int Id { get; set; }

        public FinancialType FinancialType { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }


        public string? Details { get; set; }
    }
}
