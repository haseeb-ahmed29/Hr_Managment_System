namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface IPendingRequestsReminderService
    {
        Task SendPendingRequestsSummaryAsync();
    }
}
