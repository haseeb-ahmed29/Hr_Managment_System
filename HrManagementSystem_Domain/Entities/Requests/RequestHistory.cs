using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Employees;

namespace HrMangmentSystem_Domain.Entities.Requests
{
    public class RequestHistory : TenantEntity<int>
    {
        public int GenericRequestId { get; set; }
        public GenericRequest GenericRequest { get; set; } = null!;

        public RequestAction Action { get; set; } 

        public RequestStatus? OldStatus { get; set; }

        public RequestStatus? NewStatus { get; set; }

        public string? Comment { get; set; }

        public Guid PerformedByEmployeeId { get; set; } 
        public Employee PerformedByEmployee { get; set; } = null!;


        public DateTime PerformedAt { get; set; } = DateTime.Now;

        //edit history
    }
}
