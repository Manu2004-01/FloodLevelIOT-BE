namespace Core.Interfaces
{
    public interface INotificationService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendEmailAsync(string email, string subject, string message);
    }
}
