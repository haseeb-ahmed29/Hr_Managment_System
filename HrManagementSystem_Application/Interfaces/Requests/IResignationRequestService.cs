using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Resignation;

namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface IResignationRequestService
    {
        Task<ApiResponse<bool>> ChangeResignationStatusAsync(ChangeRequestStatusDto changeRequestStatusDto);

        Task<ApiResponse<ResignationRequestDetailsDto>> CreateResignationRequestAsync(
                        CreateResignationRequestDto createResignationRequestDto);

        Task<ApiResponse<ResignationRequestDetailsDto?>> GetDetailsByIdAsync(int requestId);

        Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyResignationRequestsPagedAsync(PagedRequest request);

    }   
}
