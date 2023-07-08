using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter.Samsung
{
    public class SamsungDeviceService : IDeviceService
    {
        public string DeviceType => "Samsung";
        public SamsungDeviceService()
        {

        }
        public Task<IEnumerable<AttendanceRecord>> LoadFromDevice(DeviceInfo info, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}