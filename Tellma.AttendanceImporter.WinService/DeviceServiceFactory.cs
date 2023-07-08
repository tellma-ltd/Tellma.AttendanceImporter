using Tellma.AttendanceImporter.Contract;
using Tellma.AttendanceImporter.Samsung;
using Tellma.AttendanceImporter.Zkem;

namespace Tellma.AttendanceImporter.WinService
{
    public class DeviceServiceFactory : IDeviceServiceFactory
    {
        private readonly Dictionary<string, IDeviceService> _serviceRegistry;
        public DeviceServiceFactory(
            ZkemDeviceService zkemService,
            SamsungDeviceService samsungService
            )
        {
            _serviceRegistry = new Dictionary<string, IDeviceService>(StringComparer.OrdinalIgnoreCase)
            {
                // best practice: read dynamically from a folder and add to registry
                // when dynamic, it needs to check that device type is unique
                { zkemService.DeviceType, zkemService },
                { samsungService.DeviceType, samsungService }
            };
        }
        public IDeviceService Create(string deviceType)
        {
            if (_serviceRegistry.TryGetValue(deviceType, out IDeviceService? result)) //synatactic sugar for declaration and outputting at the same time
                return result;
            else
                throw new ArgumentException($"Unsupported device type {deviceType}");
        }
    }
}