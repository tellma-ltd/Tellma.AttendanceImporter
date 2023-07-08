using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter.Samsung.Tests
{
    public class SamsungTest
    {
        [Fact(DisplayName = "Test Samsung Device Service")]
        public async Task TestSamsungDeviceService()
        {
            // Arrange
            var samsungService = new SamsungDeviceService();
            var deviceInfo = new DeviceInfo("Samsung")
            {
                Id = 1,
                IpAddress = "127.0.0.1",
                Port = 4328,
                Name = "Test Samsung device",
                DutyStationId = 8,
                LastSyncTime = DateTime.Now.AddDays(-1)
            };
            // Act
            var records = await samsungService.LoadFromDevice(deviceInfo, CancellationToken.None);

            // Assert
            Assert.NotEmpty(records);
        }
    }
}