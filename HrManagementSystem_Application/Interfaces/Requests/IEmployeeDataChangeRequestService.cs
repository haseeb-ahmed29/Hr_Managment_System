using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface IEmployeeDataChangeRequestService
    {
        Task<ApiResponse<bool>> ChangeEmployeeDataChangeStatusAsync(ChangeRequestStatusDto changeRequestStatusDto);

        Task<ApiResponse<EmployeeDataChangeDetailsDto>> CreateEmployeeDataChangeAsync(
                         CreateEmployeeDataChangeRequestDto createEmployeeDataChangeRequestDto);

        Task<ApiResponse<EmployeeDataChangeDetailsDto?>> GetDetailsByIdAsync(int requestId);

        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyDataChangeRequestsPagedAsync(
            PagedRequest request);
    }
}

