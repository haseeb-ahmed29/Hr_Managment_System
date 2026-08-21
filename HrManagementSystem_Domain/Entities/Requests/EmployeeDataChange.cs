using HrMangmentSystem_Domain.Common;


namespace HrMangmentSystem_Domain.Entities.Requests
{
    public class EmployeeDataChange : SoftDeletable<int>
    {
        public int GenericRequestId { get; set; }
        public GenericRequest GenericRequest { get; set; } = null!;


        public string RequestedDataJson { get; set; } = null!;
        public string? ApprovedDataJson { get; set; }

   

        public DateTime? AppliedAt { get; set; }
        
       
    }
}
