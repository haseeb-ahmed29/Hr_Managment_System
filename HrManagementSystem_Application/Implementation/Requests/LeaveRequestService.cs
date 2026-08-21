using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Dto.DTOs.Requests.Leave;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Infrastructure.Implementation.Requests
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IGenericRepository<GenericRequest, int> _genericRequestRepository;
        private readonly IGenericRepository<LeaveRequest, int> _leaveRequestRepository;
        private readonly IGenericRepository<RequestHistory, int> _requestHistoryRepository;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly IRequestService _requestService;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<LeaveRequestService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        public LeaveRequestService(
            IGenericRepository<GenericRequest, int> genericRequestRepository,
            IGenericRepository<LeaveRequest, int> leaveRequestRepository,
            IGenericRepository<RequestHistory, int> requestHistoryRepository,
            ICurrentUser currentUser,
            ILogger<LeaveRequestService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IRequestService requestService,
            ILeaveBalanceService leaveBalanceService)
        {
            _genericRequestRepository = genericRequestRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _requestHistoryRepository = requestHistoryRepository;
            _currentUser = currentUser;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _requestService = requestService;
            _leaveBalanceService = leaveBalanceService;
        }

        public async Task<ApiResponse<bool>> ChangeLeaveStatusAsync(ChangeRequestStatusDto changeRequestStatusDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("ChangeLeaveStatus: Current user missing EmployeeId");
                    return ApiResponse<bool>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

              
                var request = await _genericRequestRepository
                    .Query(asNoTracking: false) // tracking
                    .FirstOrDefaultAsync(g => g.Id == changeRequestStatusDto.RequestId);

                if (request is null)
                {
                    _logger.LogWarning("ChangeLeaveStatus: Request {RequestId} not found", changeRequestStatusDto.RequestId);
                    return ApiResponse<bool>.Fail(_localizer["Request_NotFound"]);
                }

                if (request.RequestType != RequestType.LeaveRequest)
                {
                    _logger.LogWarning(
                        "ChangeLeaveStatus: Request {RequestId} is not LeaveRequest. Actual type={Type}",
                        changeRequestStatusDto.RequestId, request.RequestType);
                    return ApiResponse<bool>.Fail(_localizer["Request_InvalidType"]);
                }

                var oldStatus = request.RequestStatus;
                var newStatus = changeRequestStatusDto.NewStatus;
                var today = DateTime.Now.Date;

               
                var leave = await _leaveRequestRepository
                    .Query(asNoTracking: true)
                    .FirstOrDefaultAsync(l => l.GenericRequestId == request.Id);

                if (leave == null)
                {
                    _logger.LogError(
                        "ChangeLeaveStatus: LeaveRequest not found for GenericRequest {RequestId}",
                        request.Id);
                    return ApiResponse<bool>.Fail(_localizer["LeaveRequest_DetailsNotLoaded"]);
                }

               
                if (newStatus == RequestStatus.Approved && oldStatus != RequestStatus.Approved)
                {
                    var days = leave.TotalDays;
                    var leaveType = leave.LeaveType;
                    var employeeRequestId = request.RequestedByEmployeeId;

                    _logger.LogInformation(
                        "ChangeLeaveStatus: Trying to consume {Days} days for employee {EmployeeId}, type {LeaveType}, request {RequestId}",
                        days, employeeRequestId, leaveType, request.Id);

                    var success = await _leaveBalanceService
                        .TryConsumeLeaveAsync(employeeRequestId, leaveType, days);

                    if (!success)
                    {
                        _logger.LogWarning(
                            "ChangeLeaveStatus: Insufficient leave balance for employee {EmployeeId}, type {LeaveType}, request {RequestId}",
                            employeeRequestId, leaveType, request.Id);

                        return ApiResponse<bool>.Fail(_localizer["LeaveBalance_Insufficient"]);
                    }

                    _logger.LogInformation(
                        "ChangeLeaveStatus: Leave balance consumed for employee {EmployeeId}, request {RequestId}",
                        employeeRequestId, request.Id);
                }

               
                if (oldStatus == RequestStatus.Approved &&
                    newStatus == RequestStatus.Cancelled)
                {
                    if (today > leave.EndDate.Date)
                    {
                        _logger.LogWarning(
                            "ChangeLeaveStatus: Attempt to cancel past leave. RequestId={RequestId}, EmployeeId={EmployeeId}, [{Start} - {End}]",
                            request.Id, request.RequestedByEmployeeId,
                            leave.StartDate.Date, leave.EndDate.Date);

                        return ApiResponse<bool>.Fail(_localizer["LeaveRequest_CannotCancelPastLeave"]);
                    }

                    decimal refundDays;

                    if (today <= leave.StartDate.Date)
                    {
                       
                        refundDays = leave.TotalDays;
                    }
                    else
                    {
                        
                        var remainingDays = (leave.EndDate.Date - today).Days + 1;
                        refundDays = remainingDays > 0 ? remainingDays : 0;
                    }

                    if (refundDays > 0)
                    {
                        await _leaveBalanceService.RefundLeaveAsync(
                            request.RequestedByEmployeeId,
                            leave.LeaveType,
                            refundDays);

                        _logger.LogInformation(
                            "ChangeLeaveStatus: Refund {RefundDays} days for employee {EmployeeId}, request {RequestId}",
                            refundDays, request.RequestedByEmployeeId, request.Id);
                    }
                }

               
                return await _requestService.ChangeStatusAsync(changeRequestStatusDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ChangeLeaveStatus: Unexpected error for RequestId {RequestId}",
                    changeRequestStatusDto.RequestId);

                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<LeaveRequestDetailsDto>> CreateLeaveRequestAsync(CreateLeaveRequestDto createLeaveRequestDto)
        {
            try
            {  //add validation to  balance leave 
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Create LeaveRequest : Current User missing EmployeeId");
                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }
                if (createLeaveRequestDto.StartDate > createLeaveRequestDto.EndDate)
                {
                    _logger.LogWarning("Create LeaveRequest: StartDate > EndDate");
                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["LeaveRequest_InvalidDateRange"]);
                }
                if (createLeaveRequestDto.EndDate < DateTime.Now)
                {
                    _logger.LogWarning("Create LeaveRequest: EndDate is in the past");
                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["LeaveRequest_EndDateInPast"]);
                }
                var totalDays = (createLeaveRequestDto.EndDate.Date - createLeaveRequestDto.StartDate.Date).Days + 1;
                if (totalDays <= 0)
                {
                    _logger.LogWarning("Create LeaveRequest: Calculated TotalDays <= 0");
                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["LeaveRequest_InvalidTotalDays"]);
                }
                var hasEnoughBalance = await _leaveBalanceService.HasSufficientBalanceAsync(
                            employeeId,
                            createLeaveRequestDto.LeaveType,
                            totalDays);

                if (!hasEnoughBalance)
                {
                    _logger.LogWarning(
                        "Create LeaveRequest: Insufficient leave balance for employee {EmployeeId}. " +
                        "Requested {Days} days of type {LeaveType}",
                        employeeId, totalDays, createLeaveRequestDto.LeaveType);

                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["LeaveBalance_Insufficient"]);
                }
                var overlappingExists = await _genericRequestRepository.Query(asNoTracking: true)
                    .Include(g => g.LeaveRequest)
                    .Where(g =>
                               g.RequestType == RequestType.LeaveRequest
                           && g.RequestedByEmployeeId == employeeId
                           && (g.RequestStatus == RequestStatus.Submitted ||
                                      g.RequestStatus == RequestStatus.Approved ||
                                      g.RequestStatus == RequestStatus.InReview)
                           && g.LeaveRequest != null
                           && g.LeaveRequest.StartDate.Date <= createLeaveRequestDto.EndDate.Date
                           && g.LeaveRequest.EndDate.Date >= createLeaveRequestDto.StartDate.Date)
                    .AnyAsync();

                if (overlappingExists)
                {
                    _logger.LogWarning($"Create LeaveRequest: Overlapping leave request for employee {employeeId}");

                    return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["LeaveRequest_OverlappingDates"]);
                }

                var generic = _mapper.Map<GenericRequest>(createLeaveRequestDto);
                generic.RequestedByEmployeeId = employeeId;

                var leave = _mapper.Map<LeaveRequest>(createLeaveRequestDto);
                leave.TotalDays = totalDays;
                leave.GenericRequest = generic;

                var history = new RequestHistory
                {
                    GenericRequest = generic,
                    Action = RequestAction.Submitted,
                    OldStatus = null,
                    NewStatus = RequestStatus.Submitted,
                    Comment = null,
                    PerformedByEmployeeId = employeeId,
                    PerformedAt = DateTime.Now
                };

                await _genericRequestRepository.AddAsync(generic);
                await _leaveRequestRepository.AddAsync(leave);
                await _requestHistoryRepository.AddAsync(history);

                await _genericRequestRepository.SaveChangesAsync();


                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);
                var leaveDto = _mapper.Map<LeaveRequestDto>(leave);
                var historyDto = _mapper.Map<List<RequestHistoryDto>>(new[] { history });

                var details = new LeaveRequestDetailsDto
                {
                    History = historyDto,
                    Leave = leaveDto,
                    Request = requestDto
                };
                _logger.LogInformation($"Create LeaveRequest: Request {generic.Id} created by {employeeId}");

                return ApiResponse<LeaveRequestDetailsDto>.Ok(details, _localizer["LeaveRequest_Created"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create LeaveRequest: Unexpected error");
                return ApiResponse<LeaveRequestDetailsDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<LeaveRequestDetailsDto?>> GetDetailsByIdAsync(int requestId)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Get Leave Request Details : Current User missing EmployeeId");
                    return ApiResponse<LeaveRequestDetailsDto?>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }
                var query = _genericRequestRepository.Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Include(g => g.LeaveRequest)
                    .Include(g => g.History)
                    .ThenInclude(h => h.PerformedByEmployee)
                    .Where(g =>
                         g.Id == requestId && g.RequestType == RequestType.LeaveRequest);

                var generic = await query.FirstOrDefaultAsync();

                if (generic is null)
                {
                    _logger.LogWarning($"Get LeaveRequestDetails: Request {requestId} not found");
                    return ApiResponse<LeaveRequestDetailsDto?>.Fail(_localizer["Request_NotFound"]);
                }

                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);
                var leaveDto = _mapper.Map<LeaveRequestDto>(generic.LeaveRequest);

                var orderdHistory = generic.History
                    .OrderBy(h => h.PerformedAt)
                    .ToList();

                var historyDto = _mapper.Map<List<RequestHistoryDto>>(orderdHistory);

                var details = new LeaveRequestDetailsDto
                {
                    Request = requestDto,
                    Leave = leaveDto,
                    History = historyDto
                };
                return ApiResponse<LeaveRequestDetailsDto?>.Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get LeaveRequestDetails: Unexpected error for RequestId {requestId}");

                return ApiResponse<LeaveRequestDetailsDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyLeaveRequestsPagedAsync(PagedRequest request)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if( employeeId == Guid.Empty )
                {
                    _logger.LogWarning("GetMyLeaveRequests: Current user missing EmployeeId");
                    return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail( _localizer["Auth_EmployeeNotLinked"]);
                }
                if (request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var query = _genericRequestRepository.Query(asNoTracking: true)
                  .Include(g => g.RequestedByEmployee) 
                  .Where(g =>
                      g.RequestType == RequestType.LeaveRequest && g.RequestedByEmployeeId == employeeId);
          

                if (!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();

                    query = query.Where(g =>
                        (!string.IsNullOrEmpty(g.Title) && g.Title.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(g.Description) && g.Description.ToLower().Contains(term)));
                }

             
                if (!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    var sort = request.SortBy.Trim().ToLower();

                    query = sort switch
                    {
                        "requestedat" => request.Desc
                            ? query.OrderByDescending(g => g.RequestedAt)
                            : query.OrderBy(g => g.RequestedAt),

                        "status" => request.Desc
                            ? query.OrderByDescending(g => g.RequestStatus)
                            : query.OrderBy(g => g.RequestStatus),

                        _ => request.Desc
                            ? query.OrderByDescending(g => g.RequestedAt)
                            : query.OrderBy(g => g.RequestedAt)
                    };
                }
                else
                {
                  
                    query = query.OrderByDescending(g => g.RequestedAt);
                }

              
                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var dtoItems = _mapper.Map<List<GenericRequestListItemDto>>(items);

                var pagedResult = new PagedResult<GenericRequestListItemDto>
                {
                    Items = dtoItems,
                    TotalCount = totalCount,
                    Page = request.PageNumber,
                    PageSize = request.PageSize
                };

                _logger.LogInformation($"GetMyLeaveRequests: Loaded page {request.PageNumber} " +
                    $"(size {request.PageSize}) for employee {employeeId}");

                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Ok(pagedResult, _localizer["LeaveRequest_ListLoaded"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyLeaveRequests: Unexpected error");
                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail( _localizer["Generic_UnexpectedError"]);
            }
        }
    }
}
