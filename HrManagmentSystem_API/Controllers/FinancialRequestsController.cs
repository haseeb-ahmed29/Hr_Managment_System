using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Implementation.Requests;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Requests.Financial;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/requests/financial")]
[Authorize]
public class FinancialRequestsController : ControllerBase
{
    private readonly IFinancialRequestService _financialRequestService;

    public FinancialRequestsController( IFinancialRequestService financialRequestService)
    {
        _financialRequestService = financialRequestService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FinancialRequestDetailsDto>>> CreateFinancialRequest(
        [FromBody] CreateFinancialRequestDto request)
    {
        var result = await _financialRequestService.CreateFinancialRequestAsync(request);
        return Ok(result);
    }

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<ApiResponse<FinancialRequestDetailsDto?>>> GetFinancialRequestDetails(
        int requestId)
    {
        var result = await _financialRequestService.GetDetailsByIdAsync(requestId);
        return Ok(result);
    }

    [HttpGet("my-request")]
    public async Task<ActionResult<ApiResponse<PagedResult<GenericRequestListItemDto>>>> GetMyFinancialRequests(
        [FromQuery] PagedRequest request)
    {
        var result = await _financialRequestService.GetMyFinancialRequestsPagedAsync(request);
        return Ok(result);
    }

    [HttpPost("{requestId}/status")]
    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Manager + "," + RoleNames.SystemAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeFinancialStatus(
       int requestId,
       [FromBody] ChangeRequestStatusDto dto)
    {
        dto.RequestId = requestId;
        var result = await _financialRequestService.ChangeFinancialStatusAsync(dto);
        return Ok(result);
    }
}