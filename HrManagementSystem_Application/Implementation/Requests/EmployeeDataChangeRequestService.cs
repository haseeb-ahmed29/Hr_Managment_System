using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.PagedRequest;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Constants;
using HrMangmentSystem_Domain.Entities.Requests;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Dto.DTOs.Requests.Generic;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HrMangmentSystem_Application.Implementation.Requests
{
    public class EmployeeDataChangeRequestService : IEmployeeDataChangeRequestService
    {
        private readonly IGenericRepository<GenericRequest , int > _genericRequestRepository;
        private readonly IGenericRepository<EmployeeDataChange , int > _employeeDataChangeRepository;
        private readonly IGenericRepository<RequestHistory , int > _requestHistoryRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRequestService _requestService;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeDataChangeRequestService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public EmployeeDataChangeRequestService(
            IGenericRepository<GenericRequest, int> genericRequestRepository,
            IGenericRepository<EmployeeDataChange, int> employeeDataChangeRepository,
            IGenericRepository<RequestHistory, int> requestHistoryRepository,
            ICurrentUser currentUser,
            IMapper mapper,
            ILogger<EmployeeDataChangeRequestService> logger,
            IStringLocalizer<SharedResource> localizer,
            IRequestService requestService)
        {
            _genericRequestRepository = genericRequestRepository;
            _employeeDataChangeRepository = employeeDataChangeRepository;
            _requestHistoryRepository = requestHistoryRepository;
            _currentUser = currentUser;
            _mapper = mapper;
            _logger = logger;
            _localizer = localizer;
            _requestService = requestService;
        }

        public async Task<ApiResponse<bool>> ChangeEmployeeDataChangeStatusAsync(ChangeRequestStatusDto changeRequestStatusDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("ChangeEmployeeDataChangeStatus: Current user missing EmployeeId");
                    return ApiResponse<bool>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                var request = await _genericRequestRepository
                    .Query(asNoTracking: false)
                    .FirstOrDefaultAsync(g => g.Id == changeRequestStatusDto.RequestId);

                if (request is null)
                {
                    _logger.LogWarning("ChangeEmployeeDataChangeStatus: Request {RequestId} not found", changeRequestStatusDto.RequestId);
                    return ApiResponse<bool>.Fail(_localizer["Request_NotFound"]);
                }

                if (request.RequestType != RequestType.UpdateEmployeeData)
                {
                    _logger.LogWarning(
                        "ChangeEmployeeDataChangeStatus: Request {RequestId} type mismatch. Expected={Expected}, Actual={Actual}",
                        changeRequestStatusDto.RequestId, RequestType.UpdateEmployeeData, request.RequestType);

                    return ApiResponse<bool>.Fail(_localizer["Request_InvalidType"]);
                }

                var oldStatus = request.RequestStatus;
                var newStatus = changeRequestStatusDto.NewStatus;

               

                _logger.LogInformation(
                    "ChangeEmployeeDataChangeStatus: Changing status for Request {RequestId} from {Old} to {New}",
                    request.Id, oldStatus, newStatus);

               
                return await _requestService.ChangeStatusAsync(changeRequestStatusDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ChangeEmployeeDataChangeStatus: Unexpected error for RequestId {RequestId}",
                    changeRequestStatusDto.RequestId);

                return ApiResponse<bool>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<EmployeeDataChangeDetailsDto>> CreateEmployeeDataChangeAsync(
            CreateEmployeeDataChangeRequestDto createEmployeeDataChangeRequestDto)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Create EmployeeDataChange: Current user missing EmployeeId");
                    return ApiResponse<EmployeeDataChangeDetailsDto>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }
                if (createEmployeeDataChangeRequestDto.Changes == null || !createEmployeeDataChangeRequestDto.Changes.Any())
                {
                    _logger.LogWarning("Create EmployeeDataChange: No requested changes provided");
                    return ApiResponse<EmployeeDataChangeDetailsDto>.Fail(_localizer["EmployeeDataChange_NoChanges"]);
                }

                var generic = _mapper.Map<GenericRequest>(createEmployeeDataChangeRequestDto);
                generic.RequestedByEmployeeId = employeeId;

                var requestedJson = JsonSerializer.Serialize(createEmployeeDataChangeRequestDto.Changes);

                var dataChange = new EmployeeDataChange
                {
                    RequestedDataJson = requestedJson,
                    ApprovedDataJson = null,
                    AppliedAt = DateTime.Now,
                    GenericRequest = generic,
                };

                var history = new RequestHistory
                {
                    GenericRequest = generic,
                    OldStatus = null,
                    NewStatus = RequestStatus.Submitted,
                    Action = RequestAction.Submitted,
                    PerformedByEmployeeId = employeeId,
                    PerformedAt = DateTime.Now,
                    Comment = null
                };

                await _genericRequestRepository.AddAsync(generic);
                await _employeeDataChangeRepository.AddAsync(dataChange);
                await _requestHistoryRepository.AddAsync(history);

                await _genericRequestRepository.SaveChangesAsync();

                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);

                var requestedChanges = JsonSerializer.Deserialize<List<EmployeeDataChangeFieldDto>>(dataChange.RequestedDataJson!)
                    ?? new List<EmployeeDataChangeFieldDto>();

                var dataChangeDto = new EmployeeDataChangeDto
                {
                    Id = dataChange.Id,
                    RequestedChanges = requestedChanges,
                    ApprovedChanges = null,
                    AppliedAt = dataChange.AppliedAt
                };

                var historyDto = _mapper.Map<List<RequestHistoryDto>>(new[] { history });

                var details = new EmployeeDataChangeDetailsDto
                {
                    History = historyDto,
                    Generic = requestDto,
                    DataChange = dataChangeDto
                };

                _logger.LogInformation("Create EmployeeDataChange: Request {generic.Id} created by {employeeId}" , generic.Id , employeeId);
                return ApiResponse<EmployeeDataChangeDetailsDto>.Ok(details, _localizer["EmployeeDataChange_Created"]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create EmployeeDataChange: Unexpected error");
                return ApiResponse<EmployeeDataChangeDetailsDto>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<EmployeeDataChangeDetailsDto?>> GetDetailsByIdAsync(int requestId)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("Get EmployeeDataChangeRequest Details: Current user missing EmployeeId");
                    return ApiResponse<EmployeeDataChangeDetailsDto?>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                var query = _genericRequestRepository.Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Include(g => g.EmployeeDataChange)
                    .Include(g => g.History)
                        .ThenInclude(h => h.PerformedByEmployee)
                    .Where(g =>
                         g.Id == requestId && g.RequestType == RequestType.UpdateEmployeeData);

                var generic = await query.FirstOrDefaultAsync();

                if (generic is null)
                {
                    _logger.LogWarning("Get EmployeeDataChangeRequestDetails: Request {requestId} not found" , requestId);
                    return ApiResponse<EmployeeDataChangeDetailsDto?>.Fail(_localizer["Request_NotFound"]);
                }

                
                var requestDto = _mapper.Map<GenericRequestListItemDto>(generic);

                var dataChangeEntity = generic.EmployeeDataChange!;
                var requestdChanges = JsonSerializer.Deserialize<List<EmployeeDataChangeFieldDto>>(dataChangeEntity.RequestedDataJson!)
                    ?? new List<EmployeeDataChangeFieldDto>();
               
                List<EmployeeDataChangeFieldDto>? approvedChanges = null;
                if (!string.IsNullOrWhiteSpace(dataChangeEntity.ApprovedDataJson))
                {
                    approvedChanges = JsonSerializer.Deserialize<List<EmployeeDataChangeFieldDto>>(dataChangeEntity.ApprovedDataJson!);
                }
                var dataChangeDto = new EmployeeDataChangeDto
                {
                    Id = dataChangeEntity.Id,
                    RequestedChanges = requestdChanges,
                    ApprovedChanges = approvedChanges,
                    AppliedAt = dataChangeEntity.AppliedAt
                };

                var orderedHistory = generic.History
                    .OrderBy(h => h.PerformedAt)
                    .ToList();

                var historyDto = _mapper.Map<List<RequestHistoryDto>>(orderedHistory);

                var details = new EmployeeDataChangeDetailsDto
                {
                    Generic = requestDto,
                    DataChange = dataChangeDto,
                    History = historyDto
                };

                return ApiResponse<EmployeeDataChangeDetailsDto?>.Ok(details);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get EmployeeDataChangeRequestDetails: Unexpected error for RequestId {requestId}");

                return ApiResponse<EmployeeDataChangeDetailsDto?>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

        public async Task<ApiResponse<PagedResult<GenericRequestListItemDto>>> GetMyDataChangeRequestsPagedAsync(PagedRequest request)
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyEmployeeDataChangeRequests: Current user missing EmployeeId");
                    return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                if (request.PageNumber <= 0) request.PageNumber = 1;
                if (request.PageSize <= 0) request.PageSize = 10;

                var query = _genericRequestRepository.Query(asNoTracking: true)
                    .Include(g => g.RequestedByEmployee)
                    .Where(g =>
                        g.RequestType == RequestType.UpdateEmployeeData &&
                        g.RequestedByEmployeeId == employeeId);

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

                _logger.LogInformation($"GetMyEmployeeDataChangeRequests: Loaded page {request.PageNumber} (size {request.PageSize}) for employee {employeeId}");

                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Ok(
                    pagedResult,
                    _localizer["EmployeeDataChange_ListLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyEmployeeDataChangeRequests: Unexpected error");
                return ApiResponse<PagedResult<GenericRequestListItemDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }
    }
    
}
