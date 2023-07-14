namespace Tellma.AttendanceImporter.Contract
{
    public interface IDeviceService
    {
        public string DeviceType { get;}
        Task<IEnumerable<AttendanceRecord>> LoadFromDevice(DeviceInfo info, CancellationToken token);   
    }
}