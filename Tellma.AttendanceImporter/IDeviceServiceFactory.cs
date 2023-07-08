using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter
{
    public interface IDeviceServiceFactory
    {
        IDeviceService Create(string deviceType);
    }
}