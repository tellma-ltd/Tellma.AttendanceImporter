using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter
{
    public interface ITellmaService
    {
        Task<IEnumerable<DeviceInfo>> GetDeviceInfos(int tenantId, CancellationToken token);
        Task Import(int tenantId, IEnumerable<AttendanceRecord> records, CancellationToken token);
    }
}