using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.Financial;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface IFinancialRequestService
    {
        Task<ApiResponse<bool>> ChangeFinancialStatusAsync(ChangeRequestStatusDto changeRequestStatusDto);
        Task<ApiResponse<FinancialRequestDetailsDto>> CreateFinancialRequestAsync(CreateFinancialRequestDto createFinancialRequestDto);

        Task<ApiResponse<FinancialRequestDetailsDto?>> GetDetailsByIdAsync(int requestId);

        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyFinancialRequestsPagedAsync( PagedRequest request);
    }
}
