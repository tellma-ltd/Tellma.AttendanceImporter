using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tellma.Utilities.EmailLogger;

namespace Microsoft.Extensions.Logging
{
    public static class EmailLoggerExtensions
    {
        public static ILoggingBuilder AddEmail(this ILoggingBuilder builder, IConfiguration config)
        {
            var section = config.GetSection("Email");
            builder.Services.Configure<EmailOptions>(section);
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, EmailLoggerProvider>());
            return builder;
        }
    }
}
