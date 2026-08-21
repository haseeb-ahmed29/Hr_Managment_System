using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/requests/employee-data-change")]
[Authorize]
public class EmployeeDataChangeRequestsController : ControllerBase
{
    private readonly IEmployeeDataChangeRequestService _service;

    public EmployeeDataChangeRequestsController(IEmployeeDataChangeRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeDataChangeDetailsDto>>> CreateEmployeeDataChangeRequest(
        [FromBody] CreateEmployeeDataChangeRequestDto request)
    {
        var result = await _service.CreateEmployeeDataChangeAsync(request);
        return Ok(result);
    }

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<ApiResponse<EmployeeDataChangeDetailsDto?>>> GetEmployeeDataChangeDetails(
        int requestId)
    {
        var result = await _service.GetDetailsByIdAsync(requestId);
        return Ok(result);
    }

    [HttpGet("my-request")]
    public async Task<ActionResult<ApiResponse<PagedResult<GenericRequestListItemDto>>>> GetMyEmployeeDataChangeRequests(
        [FromQuery] PagedRequest request)
    {
        var result = await _service.GetMyDataChangeRequestsPagedAsync(request);
       
        return Ok(result);
    }
    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Manager +"," + RoleNames.SystemAdmin)]
    [HttpPost("{requestId}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeStatus(
        int requestId,
        [FromBody] ChangeRequestStatusDto dto)
    {
        dto.RequestId = requestId;
        var result = await _service.ChangeEmployeeDataChangeStatusAsync(dto);
        return Ok(result);
    }
}
