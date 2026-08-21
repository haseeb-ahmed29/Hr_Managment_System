using HrManagmentSystem_Shared.Enum.Request;

namespace HrMangmentSystem_Dto.DTOs.Requests.Generic
{
    public class GenericRequestListItemDto
    { 
        public int RequestId { get; set; }

        public RequestType RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }

        public string? Tilte {  get; set; }
        public string? Description { get; set; }

        public DateTime RequestedAt { get; set; }
        
        public Guid RequestedByEmployeeId { get; set; }
        public string RequestedByEmployeeName { get; set; } = null!;

    }
}
