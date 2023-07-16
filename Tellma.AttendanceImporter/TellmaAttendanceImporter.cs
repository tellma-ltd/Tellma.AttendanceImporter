using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tellma.AttendanceImporter.Contract;

namespace Tellma.AttendanceImporter
{
    public class TellmaAttendanceImporter
    {
        private readonly IDeviceServiceFactory _deviceServiceFactory;
        private readonly ILogger<TellmaAttendanceImporter> _logger;
        private readonly ITellmaService _tellmaService;
        private readonly IEnumerable<int> _tenantIds;

        public TellmaAttendanceImporter(IDeviceServiceFactory deviceServiceFactory, ILogger<TellmaAttendanceImporter> logger, IOptions<TellmaOptions> options)
        {
            _deviceServiceFactory = deviceServiceFactory;
            _logger = logger;
            _tellmaService = new TellmaService(options);

            _tenantIds = (options.Value.TenantIds ?? "")
                           .Split(",")
                           .Select(s =>
                           {
                               if (int.TryParse(s, out int result))
                                   return result;
                               else
                                   throw new ArgumentException($"Error parsing TenantIds config value, {s} is not a valid integer.");
                           })
                           .ToList(); // materialize for performance. Errors are thrown here.
        }
        /// <summary>
        /// Used for injecting a fake uploader for unit testing
        /// </summary>
        /// <param name="deviceServiceFactory"></param>
        /// <param name="logger"> logging function</param>
        /// <param name="tellmaService"></param>
        public TellmaAttendanceImporter(IDeviceServiceFactory deviceServiceFactory, ILogger<TellmaAttendanceImporter> logger, IOptions<TellmaOptions> options, ITellmaService tellmaService)
            : this(deviceServiceFactory, logger, options)
        {
            _tellmaService = tellmaService;
        }
        public async Task ImportToTellma(CancellationToken token)
        {
            //Stopwatch sw = Stopwatch.StartNew();

            foreach (int tenantId in _tenantIds)
            {
                IEnumerable<DeviceInfo> deviceInfos;
                try
                {
                    deviceInfos = await _tellmaService.GetDeviceInfos(tenantId, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while getting the list of device infos from tenant {tenantId}");
                    continue;
                }
                foreach (var deviceInfosOfType in deviceInfos.GroupBy(e => e.DeviceType))
                {
                    string deviceType = deviceInfosOfType.Key;
                    IDeviceService deviceService = _deviceServiceFactory.Create(deviceType);
                    foreach (DeviceInfo deviceInfo in deviceInfosOfType)
                    {
                        try
                        {
                            IEnumerable<AttendanceRecord> attendanceRecords = await deviceService.LoadFromDevice(deviceInfo, token);
                            await _tellmaService.Import(tenantId, attendanceRecords, token);
                            _logger.LogInformation($"Imported {attendanceRecords.Count()} records to Tenant {tenantId} from device ({deviceInfo})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"An error occurred while loading from device ({deviceInfo}) and uploading to tenant {tenantId}");
                            continue; // in case a new line was added later:)
                        }
                    }
                }
            }
        }
    }

}