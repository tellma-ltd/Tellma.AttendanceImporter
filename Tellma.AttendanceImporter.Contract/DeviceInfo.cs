namespace Tellma.AttendanceImporter.Contract
{
    public class DeviceInfo
    {
        public DeviceInfo(string deviceType)
        {
            DeviceType = deviceType; 
        }
        public string DeviceType { get; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? DutyStationId { get; set; }
        public string? IpAddress { get; set; }
        public int? Port { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public override string? ToString()
        {
            return Name;
        }
    }
}