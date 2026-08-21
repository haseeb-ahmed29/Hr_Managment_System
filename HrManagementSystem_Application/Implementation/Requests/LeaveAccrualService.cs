using HrManagmentSystem_Shared.Enum.Employee;
using HrManagmentSystem_Shared.Enum.Request;
using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Interfaces.Requests;
using HrMangmentSystem_Domain.Entities.Employees;
using HrMangmentSystem_Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrMangmentSystem_Application.Implementation.Requests
{
    public class LeaveAccrualService : ILeaveAccrualService
    {
        private readonly IGenericRepository<EmployeeLeaveBalance, int> _leaveBalanceRepository;
        private readonly ILogger<LeaveAccrualService> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LeaveAccrualOptions _options;

        public LeaveAccrualService(
            IStringLocalizer<SharedResource> localizer,
            ILogger<LeaveAccrualService> logger,
            IGenericRepository<EmployeeLeaveBalance, int> leaveBalanceRepository, 
            IOptions<LeaveAccrualOptions> options)
        {
            _localizer = localizer;
            _logger = logger;
            _leaveBalanceRepository = leaveBalanceRepository;
            _options = options.Value;
        }

        public async Task RunDailyAccrualAsync()
        {
            try
            {
                var today = DateTime.Now.Date;

                var query  = _leaveBalanceRepository.Query()
                    .Include(b => b.Employee)
                    .Where(b =>
                           b.LeaveType == LeaveType.Annual //Is Active User
                          && b.Employee.EmploymentStatusType == EmployeeStatus.Active
                          && b.LastUpdatedAt.Date < today);
                   
                query = query.Where(b =>
                         EF.Functions.DateDiffDay(b.LastUpdatedAt, today) > 0);

                var balancesNeedingUpdate = await  query.ToListAsync();

                if (!balancesNeedingUpdate.Any())
                {
                    _logger.LogInformation($"LeaveAccrual: No balances need update for today {today}");
                    return;
                }
                var dailyRate = 365m; 

                foreach (var balance in balancesNeedingUpdate)
                {
                
                    var daysDiff =(today - balance.LastUpdatedAt.Date).Days;

                    if (daysDiff <= 0)
                        continue;
                    
                    var added = dailyRate * daysDiff;
                    balance.BalanceDays += added;
                    balance.LastUpdatedAt = today;

                    _logger.LogInformation(
                     "LeaveAccrual: Employee {EmployeeId} +{Added} days (for {DaysDiff} days). New balance {Balance}",
                     balance.EmployeeId, added, daysDiff, balance.BalanceDays);
                }
                await _leaveBalanceRepository.SaveChangesAsync();


                _logger.LogInformation( "LeaveAccrual: {Message} at {Date}",
                    _localizer["LeaveAccrual_Completed"],today);
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "LeaveAccrual: {Message}", _localizer["LeaveAccrual_UnexpectedError"]);
            }
        }
    }
}
