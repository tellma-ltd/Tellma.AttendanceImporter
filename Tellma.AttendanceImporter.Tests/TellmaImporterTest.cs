using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter.Tests
{
    public class TellmaImporterTest
    {
        readonly TellmaOptions _options;
        public TellmaImporterTest()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<TellmaImporterTest>();
            var configuration = builder.Build();
            _options = new TellmaOptions();
            configuration.GetSection("Tellma").Bind(_options);
        }
        [Fact(DisplayName = "Test Tellma Service")]
        public async Task TestTellmaService()
        {
            // Arrange
            var importer = new TellmaAttendanceImporter(
                new MockDeviceServiceFactory(),
                new NullLogger<TellmaAttendanceImporter>(),
                Options.Create(_options)
                );

            // Act
            await importer.ImportToTellma(CancellationToken.None);

            // Assert
        }
        
        // Add another test
    }

    public class MockDeviceServiceFactory : IDeviceServiceFactory
    {
        public IDeviceService Create(string deviceType)
        {
            return new MockDeviceService(); // not using device type
        }
    }

    public class MockDeviceService : IDeviceService
    {
        public string DeviceType => "Mock";

        public Task<IEnumerable<AttendanceRecord>> LoadFromDevice(DeviceInfo info, CancellationToken token)
        {
            var result = new List<AttendanceRecord>
            {
                new AttendanceRecord(info)
                {
                    Time = new DateTime(2023,6,3,8,56,0),
                    UserId = "19",
                    IsIn = null
                }
            };

            return Task.FromResult<IEnumerable<AttendanceRecord>>(result);
        }
    }
}