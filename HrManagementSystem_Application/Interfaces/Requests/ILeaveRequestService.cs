using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Leave;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface ILeaveRequestService
    {
        Task<ApiResponse<LeaveRequestDetailsDto>> CreateLeaveRequestAsync(CreateLeaveRequestDto createLeaveRequestDto);

        Task<ApiResponse<LeaveRequestDetailsDto?>> GetDetailsByIdAsync(int requestId);
        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyLeaveRequestsPagedAsync( PagedRequest request);

        Task<ApiResponse<bool>> ChangeLeaveStatusAsync(ChangeRequestStatusDto changeRequestStatusDto);
    }
}
