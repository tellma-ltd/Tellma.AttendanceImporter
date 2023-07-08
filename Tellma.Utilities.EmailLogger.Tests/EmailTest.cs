using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Tellma.Utilities.EmailLogger.Tests
{
    public class EmailTest
    {
        private readonly EmailOptions _options;

        public EmailTest()
        {
            var builder = new ConfigurationBuilder()
     .AddUserSecrets<EmailTest>();
            var configuration = builder.Build();
            _options = new EmailOptions();
            configuration.GetSection("Email").Bind(_options);
        }
        [Fact(DisplayName = "Test Email Logger")]
        public void TestEmailLogger()
        {
            //Arrange
            var emailLogger = new EmailLogger(Options.Create(_options));
            var exception = new Exception($"Testing exception");

            //Act
            emailLogger.Log(LogLevel.Error, new EventId(50000), "", exception,
                formatter: (s, e) => e?.Message ?? "");

            //Assert
            // Check your inbox :)
        }
    }
}