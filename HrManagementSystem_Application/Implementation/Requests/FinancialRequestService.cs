using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.Financial;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HrMangmentSystem_Application.Implementation.Requests
{
    public class FinancialRequestService : IFinancialRequestService
    {
        private readonly IGenericRepository<GenericRequest, int> _genericRequestRepository;
        private readonly IGenericRepository<FinancialRequest, int> _financialRequestRepository;
        private readonly IGenericRepository<RequestHistory, int> _requestHistoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRequestService _requestService;
        private readonly ILogger<FinancialRequestService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;

        public FinancialRequestService(
            IGenericRepository<GenericRequest, int> genericRequestRepository,
            IGenericRepository<FinancialRequest, int> financialRequestRepository,
            IGenericRepository<RequestHistory, int> requestHistoryRepository,
            ICurrentUser currentUser,
            ILogger<FinancialRequestService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IRequestService requestService)
        {
            _genericRequestRepository = genericRequestRepository;
            _financialRequestRepository = financialRequestRepository;
            _requestHistoryRepository = requestHistoryRepository;
            _currentUser = currentUser;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _requestService = requestService;
        }

        public async Task<ApiResponse<bool>> ChangeFinancialStatusAsync(ChangeRequestStatusDto changeRequestStatusDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("ChangeFinancialStatus: Current user missing EmployeeId");
                    return ApiResponse<bool>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                var request = await _genericRequestRepository
                    .Query(asNoTracking: false)
                    .FirstOrDefaultAsync(g => g.Id == changeRequestStatusDto.RequestId);

                if (request is null)
                {
                    _logger.LogWarning("ChangeFinancialStatus: Request {RequestId} not found", changeRequestStatusDto.RequestId);
                    return ApiResponse<bool>.Fail(_localizer["Request_NotFound"]);
                }

                if (request.RequestType != RequestType.FinancialRequest)
                {
                    _logger.LogWarning(
                        "ChangeFinancialStatus: Request {RequestId} type mismatch. Expected={Expected}, Actual={Actual}",
                        changeRequestStatusDto.RequestId, RequestType.FinancialRequest, request.RequestType);

                    return ApiResponse<bool>.Fail(_localizer["Request_InvalidType"]);
                }

                var oldStatus = request.RequestStatus;
                var newStatus = changeRequestStatusDto.NewStatus;

                _logger.LogInformation(
                    "ChangeFinancialStatus: Changing status for Request {RequestId} from {Old} to {New}",
                    request.Id, oldStatus, newStatus);

                return await _requestService.ChangeStatusAsync(changeRequestStatusDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ChangeFinancialStatus: Unexpected error for RequestId {RequestId}",
                    changeRequestStatusDto.RequestId);

                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<FinancialRequestDetailsDto>> CreateFinancialRequestAsync(
            CreateFinancialRequestDto createFinancialRequestDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Create FinancialRequest: Current user missing EmployeeId");
                    return ApiResponse<FinancialRequestDetailsDto>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                if (createFinancialRequestDto.Amount <= 0)
                {
                    _logger.LogWarning("Create FinancialRequest: Invalid amount {Amount}", createFinancialRequestDto.Amount);
                    return ApiResponse<FinancialRequestDetailsDto>.Fail(_localizer["FinancialRequest_InvalidAmount"]);
                }

                if (createFinancialRequestDto.FromDate.HasValue && createFinancialRequestDto.ToDate.HasValue && 
                    createFinancialRequestDto.FromDate > createFinancialRequestDto.ToDate)
                {
                    _logger.LogWarning("Create FinancialRequest: Invalid date range From {From} To {To}",
                        createFinancialRequestDto.FromDate, createFinancialRequestDto.ToDate);
                    return ApiResponse<FinancialRequestDetailsDto>.Fail(_localizer["FinancialRequest_InvalidDateRange"]);
                }

                var generic = _mapper.Map<GenericRequest>(createFinancialRequestDto);
                generic.RequestedByEmployeeId = employeeId;

                var financial = _mapper.Map<FinancialRequest>(createFinancialRequestDto);
                financial.GenericRequest = generic;

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
                await _financialRequestRepository.AddAsync(financial);
                await _requestHistoryRepository.AddAsync(history);

                await _genericRequestRepository.SaveChangesAsync();

                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);
                var financialDto = _mapper.Map<FinancialRequestDto>(financial);
                var historyDto = _mapper.Map<List<RequestHistoryDto>>(new[] { history });

                var details = new FinancialRequestDetailsDto
                {
                    Request = requestDto,
                    Financial = financialDto,
                    History = historyDto
                };

                _logger.LogInformation("Create FinancialRequest: Request {RequestId} created by {EmployeeId}",
                    generic.Id, employeeId);

                return ApiResponse<FinancialRequestDetailsDto>.Ok(details, _localizer["FinancialRequest_Created"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create FinancialRequest: Unexpected error");
                return ApiResponse<FinancialRequestDetailsDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<FinancialRequestDetailsDto?>> GetDetailsByIdAsync(int requestId)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Get FinancialRequest Details: Current user missing EmployeeId");
                    return ApiResponse<FinancialRequestDetailsDto?>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                var query = _genericRequestRepository.Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Include(g => g.FinancialRequest)
                    .Include(g => g.History)
                        .ThenInclude(h => h.PerformedByEmployee)
                    .Where(g =>
                         g.Id == requestId && g.RequestType == RequestType.FinancialRequest);

                var generic = await query.FirstOrDefaultAsync();

                if (generic is null)
                {
                    _logger.LogWarning("Get FinancialRequestDetails: Request {RequestId} not found", requestId);
                    return ApiResponse<FinancialRequestDetailsDto?>.Fail(_localizer["Request_NotFound"]);
                }

            

                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);
                var financialDto = _mapper.Map<FinancialRequestDto>(generic.FinancialRequest);

                var orderedHistory = generic.History
                    .OrderBy(h => h.PerformedAt)
                    .ToList();

                var historyDto = _mapper.Map<List<RequestHistoryDto>>(orderedHistory);

                var details = new FinancialRequestDetailsDto
                {
                    Request = requestDto,
                    Financial = financialDto,
                    History = historyDto
                };

                return ApiResponse<FinancialRequestDetailsDto?>.Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Get FinancialRequestDetails: Unexpected error for RequestId {RequestId}", requestId);

                return ApiResponse<FinancialRequestDetailsDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyFinancialRequestsPagedAsync(
            PagedRequest request)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyFinancialRequests: Current user missing EmployeeId");
                    return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                if (request.PageNumber <= 0) request.PageNumber = 1;
                if (request.PageSize <= 0) request.PageSize = 10;

                var query = _genericRequestRepository.Query(asNoTracking: true)
                  .Include(g => g.RequestedByEmployee)
                  .Where(g =>
                      g.RequestType == RequestType.FinancialRequest && g.RequestedByEmployeeId == employeeId);

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

                _logger.LogInformation("GetMyFinancialRequests: Loaded page {Page} (size {Size}) for employee {EmployeeId}",
                    request.PageNumber, request.PageSize, employeeId);

                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Ok(
                    pagedResult,
                    _localizer["FinancialRequest_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyFinancialRequests: Unexpected error");
                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
    
     }
}
