using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Domain.Common;
using HrMangmentSystem_Domain.Entities.Employees;

namespace HrMangmentSystem_Domain.Entities.Requests
{
    public class GenericRequest : SoftDeletable<int>
    {
        public RequestType RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }


        public   Guid  RequestedByEmployeeId { get; set; }  // Employee who made the request
        public Employee RequestedByEmployee { get; set; } = null!;


        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedAt { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }


       public LeaveRequest? LeaveRequest { get; set; }
        public EmployeeDataChange? EmployeeDataChange { get; set; }
        public FinancialRequest? FinancialRequest { get;set; }
        public ResignationRequest? ResignationRequest { get; set; }
        
        public ICollection<RequestHistory> History { get; set; } = new List<RequestHistory>();

    }
}
