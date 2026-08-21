using HrManagmentSystem_Shared.Enum.Request;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface IRequestService
    {
        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyRequestsPagedAsync(
         PagedRequest request,
         RequestType? filterByType = null);

        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetRequestsForApprovalPagedAsync(
        PagedRequest request,
        RequestType? filterByType = null);

        Task<ApiResponse<GenericRequestListItemDto?>> GetRequestHeaderByIdAsync(int requestId);

        Task<ApiResponse<List<RequestHistoryDto>>> GetRequestHistoryAsync(int requestId);

        Task<ApiResponse<bool>> ChangeStatusAsync(ChangeRequestStatusDto changeRequestStatusDto);

    }
}
