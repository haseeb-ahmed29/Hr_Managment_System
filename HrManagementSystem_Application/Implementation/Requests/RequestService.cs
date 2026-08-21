using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Requests
{
    public class RequestService : IRequestService
    {
        private readonly IGenericRepository<GenericRequest, int> _genericRequestRepository;
        private readonly IGenericRepository<RequestHistory, int> _requestHistoryRepository;
        private readonly IGenericRepository<LeaveRequest, int> _leaveRequestRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<RequestService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public RequestService(
            IGenericRepository<GenericRequest, int> genericRequestRepository,
            IGenericRepository<RequestHistory, int> requestHistoryRepository,
            ICurrentUser currentUser,
            ILogger<RequestService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            ILeaveBalanceService leaveBalanceService,
            IGenericRepository<LeaveRequest, int> leaveRequestRepository)
        {
            _genericRequestRepository = genericRequestRepository;
            _requestHistoryRepository = requestHistoryRepository;
            _currentUser = currentUser;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _leaveBalanceService = leaveBalanceService;
            _leaveRequestRepository = leaveRequestRepository;
        }



        public async Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyRequestsPagedAsync(
            PagedRequest request,
            RequestType? filterByType = null)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyRequests: Current user missing EmployeeId");
                    return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(
                        _localizer["Auth_EmployeeNotLinked"]);
                }

                if (request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var query = _genericRequestRepository
                    .Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Where(g => g.RequestedByEmployeeId == employeeId);

                if (filterByType.HasValue)
                {
                    query = query.Where(g => g.RequestType == filterByType.Value);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();
                    query = query.Where(g =>
                        (!string.IsNullOrEmpty(g.Title) && g.Title.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(g.Description) && g.Description.ToLower().Contains(term)));
                }

                // Sorting
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

                _logger.LogInformation(
                    "GetMyRequests: Loaded page {Page} (size {Size}) for employee {EmployeeId}",
                    request.PageNumber, request.PageSize, employeeId);

                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Ok(
                    pagedResult,
                    _localizer["Request_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyRequests: Unexpected error");
                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(
                    _localizer["Generic_UnexpectedError"]);
            }
        }



        public async Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetRequestsForApprovalPagedAsync(
            PagedRequest request,
            RequestType? filterByType = null)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetRequestsForApproval: Current user missing EmployeeId");
                    return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(
                        _localizer["Auth_EmployeeNotLinked"]);
                }


             

                if (request.PageNumber <= 0)
                    request.PageNumber = 1;

                if (request.PageSize <= 0)
                    request.PageSize = 10;

                var query = _genericRequestRepository
                    .Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Where(g =>
                        g.RequestStatus == RequestStatus.Submitted ||
                        g.RequestStatus == RequestStatus.InReview);

                if (filterByType.HasValue)
                {
                    query = query.Where(g => g.RequestType == filterByType.Value);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(request.Term))
                {
                    var term = request.Term.Trim().ToLower();
                    query = query.Where(g =>
                        (!string.IsNullOrEmpty(g.Title) && g.Title.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(g.Description) && g.Description.ToLower().Contains(term)));
                }

                // Sorting
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

                _logger.LogInformation(
                    "GetRequestsForApproval: Loaded page {Page} (size {Size}) for approver {EmployeeId}",
                    request.PageNumber, request.PageSize, employeeId);

                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Ok(
                    pagedResult,
                    _localizer["Request_ApprovalListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRequestsForApproval: Unexpected error");
                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(
                    _localizer["Generic_UnexpectedError"]);
            }
        }


        public async Task<ApiResponse<GenericRequestListItemDto?>> GetRequestHeaderByIdAsync(int requestId)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetRequestHeader: Current user missing EmployeeId");
                    return ApiResponse<GenericRequestListItemDto?>.Fail(
                        _localizer["Auth_EmployeeNotLinked"]);
                }

                var query = _genericRequestRepository
                    .Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Where(g => g.Id == requestId);

                var generic = await query.FirstOrDefaultAsync();

                if (generic is null)
                {
                    _logger.LogWarning("GetRequestHeader: Request {RequestId} not found", requestId);
                    return ApiResponse<GenericRequestListItemDto?>.Fail(
                        _localizer["Request_NotFound"]);
                }


            

                var dto = _mapper.Map<GenericRequestListItemDto>(generic);

                return ApiResponse<GenericRequestListItemDto?>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetRequestHeader: Unexpected error for RequestId {RequestId}", requestId);

                return ApiResponse<GenericRequestListItemDto?>.Fail(
                    _localizer["Generic_UnexpectedError"]);
            }
        }



        public async Task<ApiResponse<List<RequestHistoryDto>>> GetRequestHistoryAsync(int requestId)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetRequestHistory: Current user missing EmployeeId");
                    return ApiResponse<List<RequestHistoryDto>>.Fail(
                        _localizer["Auth_EmployeeNotLinked"]);
                }

                var query = _genericRequestRepository
                    .Query(asNoTracking: true)
                    .Include(g => g.History)
                        .ThenInclude(h => h.PerformedByEmployee)
                    .Where(g => g.Id == requestId);

                var generic = await query.FirstOrDefaultAsync();

                if (generic is null)
                {
                    _logger.LogWarning("GetRequestHistory: Request {RequestId} not found", requestId);
                    return ApiResponse<List<RequestHistoryDto>>.Fail(
                        _localizer["Request_NotFound"]);
                }

                var orderedHistory = generic.History
                    .OrderBy(h => h.PerformedAt)
                    .ToList();

                var dto = _mapper.Map<List<RequestHistoryDto>>(orderedHistory);

                return ApiResponse<List<RequestHistoryDto>>.Ok(
                    dto,
                    _localizer["Request_HistoryLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetRequestHistory: Unexpected error for RequestId {RequestId}", requestId);

                return ApiResponse<List<RequestHistoryDto>>.Fail(
                    _localizer["Generic_UnexpectedError"]);
            }
        }



        public async Task<ApiResponse<bool>> ChangeStatusAsync(ChangeRequestStatusDto changeRequestStatusDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("ChangeStatus: Current user missing EmployeeId");
                    return ApiResponse<bool>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

               

                var request = await _genericRequestRepository
                    .Query(asNoTracking :false ) // tracking
                    .Include(g => g.History)
                    .FirstOrDefaultAsync(g => g.Id == changeRequestStatusDto.RequestId);

                if (request is null)
                {
                    _logger.LogWarning("ChangeStatus: Request {RequestId} not found",
                        changeRequestStatusDto.RequestId);
                    return ApiResponse<bool>.Fail(_localizer["Request_NotFound"]);
                }

                var newStatus = changeRequestStatusDto.NewStatus;
                var oldStatus = request.RequestStatus;

                if (!IsTransitionAllowed(oldStatus, newStatus))
                {
                    _logger.LogWarning("ChangeStatus: Invalid transition {Old} -> {New} for Request {RequestId}",
                        request.RequestStatus, newStatus, request.Id);

                    return ApiResponse<bool>.Fail(
                        _localizer["Request_StatusChange_InvalidTransition",
                            request.RequestStatus.ToString(), newStatus.ToString()]);
                }
           
               

                request.RequestStatus = newStatus;
                request.LastUpdatedAt = DateTime.Now;

                var history = new RequestHistory
                {
                    GenericRequestId = request.Id,
                    Action = MapStatusToAction(newStatus),
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Comment = changeRequestStatusDto.Comment,
                    PerformedByEmployeeId = employeeId,
                    PerformedAt = DateTime.Now
                };
                 _genericRequestRepository.Update(request);
                await _requestHistoryRepository.AddAsync(history);

                await _genericRequestRepository.SaveChangesAsync();

                _logger.LogInformation("ChangeStatus: Request {RequestId} from {Old} to {New} by {EmployeeId}",
                    request.Id, oldStatus, newStatus, employeeId);

                return ApiResponse<bool>.Ok(true, _localizer["Request_StatusChanged"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ChangeStatus: Unexpected error for RequestId {RequestId}",
                    changeRequestStatusDto.RequestId);

                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }



        private bool IsTransitionAllowed(RequestStatus oldStatus, RequestStatus newStatus)
        {
            //tuple 
            return (oldStatus, newStatus) switch
            {
                (RequestStatus.Draft, RequestStatus.Submitted) => true,
                (RequestStatus.Submitted, RequestStatus.InReview) => true,
                (RequestStatus.Submitted, RequestStatus.Cancelled) => true,
                (RequestStatus.InReview, RequestStatus.Approved) => true,
                (RequestStatus.InReview, RequestStatus.Rejected) => true,
                (RequestStatus.Approved, RequestStatus.Cancelled) => true,
                _ => false
            };
        }

        private RequestAction MapStatusToAction(RequestStatus status)
        {
            return status switch
            {
                RequestStatus.Submitted => RequestAction.Submitted,
                RequestStatus.Rejected => RequestAction.Rejected,
                RequestStatus.Cancelled => RequestAction.Cancelled,
                _ => RequestAction.StatusChanged
            };
        }

       
       
        
    }
}
