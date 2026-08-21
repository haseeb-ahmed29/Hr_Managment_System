using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Leave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/requests/leave")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ILeaveBalanceService _leaveBalanceService;

    public LeaveRequestsController(ILeaveRequestService leaveRequestService, ILeaveBalanceService leaveBalanceService)
    {
        _leaveRequestService = leaveRequestService;
        _leaveBalanceService = leaveBalanceService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeaveRequestDetailsDto>>> CreateLeaveRequest(
        [FromBody] CreateLeaveRequestDto request)
    {
        var result = await _leaveRequestService.CreateLeaveRequestAsync(request);
        return Ok(result);
    }

    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDetailsDto?>>> GetLeaveRequestDetails(int requestId)
    {
        var result = await _leaveRequestService.GetDetailsByIdAsync(requestId);
        return Ok(result);
    }

    [HttpGet("my-request")]
    public async Task<ActionResult<ApiResponse<PagedResult<GenericRequestListItemDto>>>> GetMyLeaveRequests(
        [FromQuery] PagedRequest request)
    {
        var result = await _leaveRequestService.GetMyLeaveRequestsPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("my-balance")]
    public async Task<ActionResult<ApiResponse<List<EmployeeLeaveBalanceDto>>>> GetMyLeaveBalance()
    {
        var result = await _leaveBalanceService.GetMyLeaveBalanceAsync();
        return Ok(result);
    }

    [HttpPost("{requestId}/status")]
    [Authorize(Roles = RoleNames.HrAdmin + "," + RoleNames.Manager + "," + RoleNames.SystemAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeLeaveStatus(
      int requestId,
      [FromBody] ChangeRequestStatusDto dto)
    {
        dto.RequestId = requestId;
        var result = await _leaveRequestService.ChangeLeaveStatusAsync(dto);
        return Ok(result);
    }
}