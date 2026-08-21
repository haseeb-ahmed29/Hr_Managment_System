using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Dto.DTOs.Requests.EmployeeData
{
    public class EmployeeDataChangeDetailsDto
    {
        public GenericRequestListItemDto Generic { get; set; } = null!;

        public EmployeeDataChangeDto DataChange { get; set; } = null!;

        public List<RequestHistoryDto> History { get; set; } = new();
    }
}
