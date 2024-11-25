using Microsoft.Extensions.Logging.Abstractions;
using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter.Zkem.Tests
{
    public class ZkemTest
    {
        // Add another test
        [Fact(DisplayName = "Test Zkem Device Service")]
        public async Task TestZkemDeviceService()
        {
            // Arrange
            var zkemService = new ZkemDeviceService(new NullLogger<ZkemDeviceService>());
            var deviceInfo = new DeviceInfo("Zkem")
            {
                // put real data below
                Id = 1,
                IpAddress = "135.181.51.50",
                Port = 8107, // was 10
                Name = "Test Zkem device",
                DutyStationId = 8,
                LastSyncTime = new DateTime(2023, 7, 6)
            };
            // Act
            var records = await zkemService.LoadFromDevice(deviceInfo, CancellationToken.None);

            // Assert
            Assert.NotEmpty(records);
        }
    }
}