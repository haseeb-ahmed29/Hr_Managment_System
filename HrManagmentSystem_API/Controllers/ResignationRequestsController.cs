using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Resignation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/requests/resignation")]
[Authorize]

public class ResignationRequestsController : ControllerBase
{
    private readonly IResignationRequestService _service;

    public ResignationRequestsController(IResignationRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ResignationRequestDetailsDto>>> CreateResignationRequest(
        [FromBody] CreateResignationRequestDto request)
    {
        var result = await _service.CreateResignationRequestAsync(request);
        return Ok(result);
    }

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<ApiResponse<ResignationRequestDetailsDto?>>> GetResignationRequestDetails(
        int requestId)
    {
        var result = await _service.GetDetailsByIdAsync(requestId);
        return Ok(result);
    }

    [HttpGet("my-request")]
    public async Task<ActionResult<ApiResponse<PagedResult<GenericRequestListItemDto>>>> GetMyResignationRequests(
        [FromQuery] PagedRequest request)
    {
        var result = await _service.GetMyResignationRequestsPagedAsync(request);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Manager + "," + RoleNames.SystemAdmin)]
    [HttpPost("{requestId}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeStatus(
        int requestId,
        [FromBody] ChangeRequestStatusDto dto)
    {
        dto.RequestId = requestId;
        var result = await _service.ChangeResignationStatusAsync(dto);
        return Ok(result);
    }
}