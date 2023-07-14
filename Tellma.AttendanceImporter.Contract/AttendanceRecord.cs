namespace Tellma.AttendanceImporter.Contract
{
    public class AttendanceRecord
    {
        public AttendanceRecord(DeviceInfo deviceInfo)
        {
            DeviceInfo = deviceInfo ?? throw new ArgumentNullException(nameof(deviceInfo));
        }
        /// <summary>
        /// Retrieved from Resource in Tellma: TNA Device
        /// </summary>
        public DeviceInfo DeviceInfo { get; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public bool? IsIn { get; set; }
    }
}