using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Dto.DTOs.Requests.Financial;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Leave;
using HrMangmentSystem_Dto.DTOs.Requests.Resignation;

namespace HrMangmentSystem_Application.Mapper
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
            CreateMap<GenericRequest , GenericRequestListItemDto>()
                .ForMember(dest => dest.RequestId,
                     opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RequestedByEmployeeName,
                     opt => opt.MapFrom(src =>
                       src.RequestedByEmployee.FirstName + " " + src.RequestedByEmployee.LastName));


            CreateMap<LeaveRequest, LeaveRequestDto>().ReverseMap() ;
            CreateMap<CreateLeaveRequestDto, LeaveRequest>();
            CreateMap<CreateLeaveRequestDto, GenericRequest>()
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(_ => RequestType.LeaveRequest))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Submitted))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => $"Leave Request ({src.LeaveType})"))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.RequestedAt, opt=> opt.MapFrom(_ => DateTime.Now));


            CreateMap<EmployeeDataChange, EmployeeDataChangeDto>()
             .ForMember(d => d.RequestedChanges, opt => opt.Ignore())
             .ForMember(d => d.ApprovedChanges, opt => opt.Ignore());

            CreateMap<CreateEmployeeDataChangeRequestDto, GenericRequest>()
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(_ => RequestType.UpdateEmployeeData))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Submitted))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Title)
                    ? "Employee data change request"
                    : src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.Now));


            CreateMap<FinancialRequest, FinancialRequestDto>();
            CreateMap<CreateFinancialRequestDto, FinancialRequest>();
            CreateMap<CreateFinancialRequestDto, GenericRequest>()
              .ForMember(dest => dest.RequestType, opt => opt.MapFrom(_ => RequestType.FinancialRequest))
              .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Submitted))
              .ForMember(dest => dest.Title, opt => opt.MapFrom(src =>
                  string.IsNullOrWhiteSpace(src.Title)
                      ? $"Financial Request ({src.FinancialType})"
                      : src.Title))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.Now));


            CreateMap<ResignationRequest, ResignationRequestDto>().ReverseMap();
            CreateMap<CreateResignationRequestDto, ResignationRequest>();
            CreateMap<CreateResignationRequestDto, GenericRequest>()
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(_ => RequestType.ResignationRequest))
                .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(_ => RequestStatus.Submitted))
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.Now));


            CreateMap<RequestHistory, RequestHistoryDto>()
               .ForMember(dest => dest.PerformedByEmployeeName,
                   opt => opt.MapFrom(src =>
                       src.PerformedByEmployee.FirstName + " " + src.PerformedByEmployee.LastName));


            CreateMap<EmployeeLeaveBalance, EmployeeLeaveBalanceDto>();
        }

    }
}
