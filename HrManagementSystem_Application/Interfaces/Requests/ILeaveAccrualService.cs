namespace HrMangmentSystem_Application.Interfaces.Requests
{
    public interface ILeaveAccrualService
    {
        public Task RunDailyAccrualAsync();
    }
}
