using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Dto.DTOs.Requests.Financial
{
    public class FinancialRequestDetailsDto
    {
        public GenericRequestListItemDto Request { get; set; } = null!;
        public FinancialRequestDto Financial { get; set; } = null!;

        public List<RequestHistoryDto> History { get; set; } = new();
    }
}
