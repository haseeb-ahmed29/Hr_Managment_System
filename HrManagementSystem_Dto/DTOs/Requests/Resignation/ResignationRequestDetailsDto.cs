using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Dto.DTOs.Requests.Resignation
{
    public class ResignationRequestDetailsDto
    {
        public GenericRequestListItemDto Request { get; set; } = null!;

        public ResignationRequestDto Resignation { get; set; } = null!;

        public List<RequestHistoryDto> History { get; set; } = new();
    }
}
