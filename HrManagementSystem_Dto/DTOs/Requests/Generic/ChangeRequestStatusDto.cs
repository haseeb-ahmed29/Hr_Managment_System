using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.Generic
{
    public class ChangeRequestStatusDto
    {
        public int RequestId { get; set; }

        public RequestStatus NewStatus { get; set; }

        public string? Comment { get; set; }
    }
}
