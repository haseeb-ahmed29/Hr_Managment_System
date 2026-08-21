namespace HrMangmentSystem_Application.Interfaces.Notifications
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to ,string subject , string body);
    }
}
