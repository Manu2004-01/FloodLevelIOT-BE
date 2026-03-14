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
        
        public NotificationService(IEmailProvider emailProvider)
        {
            _emailProvider = emailProvider;
        }
        public Task SendEmailAsync(string email, string subject, string message)
            => _emailProvider.SendEmailAsync(email, subject, message);
    }
}
