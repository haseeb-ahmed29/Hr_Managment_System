using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.Financial
{
    public class CreateFinancialRequestDto
    {
        public FinancialType FinancialType { get; set; }

        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "JOD";


        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }


        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? Details { get; set; }
    }
}
