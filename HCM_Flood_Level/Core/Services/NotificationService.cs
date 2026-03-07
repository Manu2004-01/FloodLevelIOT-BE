using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailProvider _emailProvider;
        private readonly ISmsProvider _smsProvider;
        public NotificationService(IEmailProvider emailProvider, ISmsProvider smsProvider)
        {
            _emailProvider = emailProvider;
            _smsProvider = smsProvider;
        }
        public Task SendEmailAsync(string email, string subject, string message)
            => _emailProvider.SendEmailAsync(email, subject, message);
        public Task SendSmsAsync(string phoneNumber, string message)
            => _smsProvider.SendSmsAsync(phoneNumber, message);
    }
}
