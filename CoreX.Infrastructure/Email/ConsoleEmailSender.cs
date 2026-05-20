using CoreX.Application.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace CoreX.Infrastructure.Email
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation(
                "[EMAIL STUB] To: {ToEmail} | Subject: {Subject} | Body: {Body}",
                toEmail, subject, body);

            return Task.CompletedTask;
        }
    }
}
