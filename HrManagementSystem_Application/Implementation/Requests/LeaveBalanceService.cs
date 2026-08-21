using AutoMapper;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Common.Responses;
using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Dto.DTOs.Requests.EmployeeData;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using HrMangmentSystem_Infrastructure.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace HrMangmentSystem_Application.Implementation.Requests
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly IGenericRepository<EmployeeLeaveBalance, int> _leaveBalanceRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<LeaveBalanceService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly LeaveBalanceOptions _leaveBalanceOptions;
        public LeaveBalanceService(
           IGenericRepository<EmployeeLeaveBalance, int> leaveBalanceRepository,
           ICurrentUser currentUser,
           ILogger<LeaveBalanceService> logger,
           IStringLocalizer<SharedResource> localizer,
           IMapper mapper,
           IOptions<LeaveBalanceOptions> Options)
        {
            _leaveBalanceRepository = leaveBalanceRepository;
            _currentUser = currentUser;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _leaveBalanceOptions = Options.Value;
        }

        public async Task<ApiResponse<List<EmployeeLeaveBalanceDto>>> GetMyLeaveBalanceAsync()
        {
            try
            {
                var employeeId = _currentUser.EmployeeId;
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyLeaveBalance: Current user missing EmployeeId");
                    return ApiResponse<List<EmployeeLeaveBalanceDto>>.Fail(_localizer["Auth_EmployeeNotLinked"]);
                }

                var query = _leaveBalanceRepository
                    .Query(asNoTracking: true)
                    .Where(b => b.EmployeeId == employeeId);

                var items = await query.ToListAsync();

                var dto = _mapper.Map<List<EmployeeLeaveBalanceDto>>(items);

                _logger.LogInformation("GetMyLeaveBalance: Loaded {Count} leave balance records for employee {EmployeeId}"
                    , dto.Count, employeeId);
                return ApiResponse<List<EmployeeLeaveBalanceDto>>.Ok(dto,_localizer["LeaveBalance_MyLoaded"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyLeaveBalance: Unexpected error");

                return ApiResponse<List<EmployeeLeaveBalanceDto>>.Fail(_localizer["Generic_UnexpectedError"]);
            }
        }

       
        public async Task InitializeAnnualLeaveForEmployeeAsync(Guid employeeId, DateTime employmentStartDate)
        {
            try
            {
                if (employeeId == Guid.Empty)
                {
                    _logger.LogWarning("InitializeAnnualLeave: EmployeeId is empty");
                    return;
                }

                var annualExists = await _leaveBalanceRepository
                    .Query(asNoTracking: true)
                    .AnyAsync(b => b.EmployeeId == employeeId && b.LeaveType == LeaveType.Annual);

                if (!annualExists)
                {
                    var annualbalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveType = LeaveType.Annual,
                        BalanceDays = 0m, //start zero (add by job)
                        LastUpdatedAt = employmentStartDate,
                        CreatedAt = DateTime.Now,
                        CreatedBy = Guid.Empty
                    };

                    await _leaveBalanceRepository.AddAsync(annualbalance);

                    _logger.LogInformation($"InitializeAnnualLeave: Created annual leave balance for employee {employeeId} " +
                   $"starting from {employmentStartDate}");
                }
                else
                {
                    _logger.LogInformation($"InitializeAnnualLeave: Annual leave balance already exists for employee {employeeId}. Skipping creation.");
                }
                var sickExists = await _leaveBalanceRepository
                       .Query(asNoTracking: true)
                       .AnyAsync(b => b.EmployeeId == employeeId &&
                        b.LeaveType == LeaveType.Sick);

                if (!sickExists)
                {
                    var initialSick = _leaveBalanceOptions.InitialSickLeaveDays;

                    var sickBalance = new EmployeeLeaveBalance
                    {
                        EmployeeId = employeeId,
                        LeaveType = LeaveType.Sick,
                        BalanceDays = initialSick,
                        LastUpdatedAt = employmentStartDate,
                        CreatedAt = DateTime.Now,
                        CreatedBy = Guid.Empty
                    };

                    await _leaveBalanceRepository.AddAsync(sickBalance);

                    _logger.LogInformation(
                       "InitializeAnnualLeave: Created sick leave balance for employee {EmployeeId} with {Days} days starting from {HireDate}",
                           employeeId, initialSick, employmentStartDate);
                }
                else
                {
                    _logger.LogInformation($"InitializeAnnualLeave: Sick leave balance already exists for employee {employeeId}. Skipping creation.");
                }
                
                                await _leaveBalanceRepository.SaveChangesAsync();
            }          
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InitializeAnnualLeave: Unexpected error for employee {employeeId}");
            }
        }

    
        public async Task<bool> TryConsumeLeaveAsync(Guid employeeId, LeaveType leaveType, decimal days)
        {
            try
            {
                if (!IsBalanceRequired(leaveType))
                {
                    _logger.LogDebug(
                        "TryConsumeLeave: LeaveType {LeaveType} does not require balance. Skipping consume.",
                        leaveType);
                    return true; 
                }
                if (employeeId == Guid.Empty || days <= 0)
                {
                    _logger.LogWarning($"TryConsumeLeave: Invalid parameters. EmployeeId={employeeId}, Days={days}");
                    return false;
                }

                var balance = await _leaveBalanceRepository
                    .Query() // tracking
                    .FirstOrDefaultAsync(b =>
                        b.EmployeeId == employeeId &&
                        b.LeaveType == leaveType);

                if (balance is null)
                {
                    _logger.LogWarning($"TryConsumeLeave: No balance record for employee {employeeId}, type {leaveType}");
                    return false;
                }

                if (balance.BalanceDays < days)
                {
                    _logger.LogWarning(
                        "TryConsumeLeave: Insufficient balance for employee {EmployeeId}, type {LeaveType}. Current {Current}, Required {Required}",
                        employeeId, leaveType, balance.BalanceDays, days);

                    return false;
                }

                balance.BalanceDays -= days;
                balance.LastUpdatedAt = DateTime.Now;

                await _leaveBalanceRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "TryConsumeLeave: Consumed {Days} days from employee {EmployeeId} for type {LeaveType}. New balance {Balance}",
                    days, employeeId, leaveType, balance.BalanceDays);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "TryConsumeLeave: Unexpected error for employee {EmployeeId}, type {LeaveType}",
                    employeeId, leaveType);

                return false;
            }
        }


        public async Task RefundLeaveAsync(Guid employeeId, LeaveType leaveType, decimal days)
        {
            try
            {
                if (!IsBalanceRequired(leaveType))
                {
                    _logger.LogDebug(
                        "RefundLeave: LeaveType {LeaveType} does not require balance. Skipping refund.",
                        leaveType);
                    return;
                }
                if (employeeId == Guid.Empty || days <= 0)
                {
                    _logger.LogWarning(
                        "RefundLeave: Invalid parameters. EmployeeId={EmployeeId}, Days={Days}",
                        employeeId, days);
                    return;
                }

                var balance = await _leaveBalanceRepository
                    .Query() // tracking
                    .FirstOrDefaultAsync(b =>
                        b.EmployeeId == employeeId &&
                        b.LeaveType == leaveType);

                if (balance is null)
                {
                    _logger.LogWarning(
                        "RefundLeave: No balance record for employee {EmployeeId}, type {LeaveType}",
                        employeeId, leaveType);
                    return;
                }

                balance.BalanceDays += days;
                balance.LastUpdatedAt = DateTime.Now;

                await _leaveBalanceRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "RefundLeave: Refunded {Days} days to employee {EmployeeId} for type {LeaveType}. New balance {Balance}",
                    days, employeeId, leaveType, balance.BalanceDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "RefundLeave: Unexpected error for employee {EmployeeId}, type {LeaveType}",
                    employeeId, leaveType);
            }
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid employeeId, LeaveType leaveType, decimal days)
        {
            try
            {
                if (!IsBalanceRequired(leaveType))
                {
                    _logger.LogDebug(
                        "HasSufficientBalance: LeaveType {LeaveType} does not require balance. Returning true.",
                        leaveType);
                    return true;
                }

                var balance = await _leaveBalanceRepository
                    .Query(asNoTracking: true)
                    .FirstOrDefaultAsync(b =>
                        b.EmployeeId == employeeId &&
                        b.LeaveType == leaveType);

                if (balance is null)
                {
                    _logger.LogWarning(
                        "HasSufficientBalance: No balance record found for employee {EmployeeId}, type {LeaveType}",
                        employeeId, leaveType);

                    return false; 
                }

                var hasEnough = balance.BalanceDays >= days;

                _logger.LogInformation(
                    "HasSufficientBalance: Employee {EmployeeId}, type {LeaveType}, " +
                    "current={Current}, requested={Requested}, result={Result}",
                    employeeId, leaveType, balance.BalanceDays, days, hasEnough);

                return hasEnough;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "HasSufficientBalance: Unexpected error for employee {EmployeeId}, type {LeaveType}",
                    employeeId, leaveType);

               
                return false;
            }
        }

        private bool IsBalanceRequired(LeaveType leaveType)
        {
            return leaveType == LeaveType.Annual
                || leaveType == LeaveType.Sick;
        }
    }
}
    

