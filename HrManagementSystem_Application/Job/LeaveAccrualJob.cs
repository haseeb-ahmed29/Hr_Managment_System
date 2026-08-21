using HrManagmentSystem_Shared.Resources;
using HrMangmentSystem_Application.Interfaces.Requests;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HrMangmentSystem_Application.Job
{
    public class LeaveAccrualJob : IJob
    {
        private readonly ILeaveAccrualService _leaveAccrualService;
        private readonly ILogger<LeaveAccrualJob> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LeaveAccrualJob(IStringLocalizer<SharedResource> localizer, ILeaveAccrualService leaveAccrualService, ILogger<LeaveAccrualJob> logger)
        {
            _localizer = localizer;
            _leaveAccrualService = leaveAccrualService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.Now;

            _logger.LogInformation(
                "LeaveAccrualJob: {Message} at {Time}",
                _localizer["LeaveAccrualJob_Started"], now);

            await _leaveAccrualService.RunDailyAccrualAsync();

            _logger.LogInformation(
                "LeaveAccrualJob: {Message} at {Time}",
                _localizer["LeaveAccrualJob_Finished"], DateTime.Now);
        }
    }
}
