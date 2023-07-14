using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Tellma.Utilities.EmailLogger
{
    public class EmailLogger : ILogger
    {
        private readonly EmailOptions _options;
        private readonly IEnumerable<string> _emails;

        public EmailLogger(EmailOptions options)
        {
            _options = options;
            _emails = (_options.EmailAddresses ?? "").Split(",").Select(s => s.Trim()).ToList();
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: check that email addresses are valid
            return (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
                && _emails.Any();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (exception == null) return;
            if (!IsEnabled(logLevel)) return;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Email Logger", "donotreply@tellma.com"));
                foreach (var email in _emails)
                    message.To.Add(new MailboxAddress(email, email));
                message.Subject = $"{_options.InstallationIdentifier ?? "Unknown"}: Unhandled {exception.GetType().Name}: {Truncate(exception.Message, 50, true)}";

                message.Body = new TextPart("plain")
                {
                    Text = $@"
{formatter(state, exception)}

--- Stack Trace ---

{exception}"
                };

                using var client = new SmtpClient();
                client.Connect(_options.SmtpHost, _options.SmtpPort ?? 587, _options.SmtpUseSsl);

                // Note: only needed if the SMTP server requires authentication
                if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
                    client.Authenticate(_options.SmtpUsername, _options.SmtpPassword);

                client.Send(message);
                client.Disconnect(true);
            }
            catch (Exception e)
            { 
                
            }
        }

        /// <summary>
        /// Removes all characters after a certain length.
        /// </summary>
        public static string Truncate(string value, int maxLength, bool appendEllipses = false)
        {
            const string ellipses = "...";

            if (maxLength < 0)
            {
                return value;
            }
            else if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            else if (value.Length <= maxLength)
            {
                return value;
            }
            else
            {
                var truncated = value.Substring(0, maxLength);
                if (appendEllipses)
                {
                    truncated += ellipses;
                }

                return truncated;
            }
        }
    }
}
