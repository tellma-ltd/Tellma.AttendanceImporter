using Microsoft.Extensions.Logging;
using System.Transactions;
using Tellma.AttendanceImporter.Contract;
using zkemkeeper;

namespace Tellma.AttendanceImporter.Zkem
{
    public class ZkemDeviceService : IDeviceService
    {
        private readonly ILogger<ZkemDeviceService> logger;
        private const string LOG_DATE_FORMAT = "hh:mm:ss";

        public string DeviceType => "Zkem"; // syntactic sugar for defining a getter property only

        public ZkemDeviceService(ILogger<ZkemDeviceService> logger)
        {
            this.logger = logger;
        }
        public async Task<IEnumerable<AttendanceRecord>> LoadFromDevice(DeviceInfo deviceInfo, CancellationToken token)
        {
            CZKEM deviceClient = new CZKEM();
            IList<AttendanceRecord> list = new List<AttendanceRecord>();
            int workCode = 0;
            bool isConnected = false;

            try
            {
                isConnected = deviceClient.Connect_Net(deviceInfo.IpAddress, deviceInfo.Port ?? 0) ? true
                    : ThrowLastError(deviceInfo, deviceClient, "Connect", true);

                logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Connected");
                //      deviceClient.Disable(deviceClient);
                //      logger(string.Format("{0} - Location({1}): Disabled", DateTime.Now.ToString(LOG_DATE_FORMAT), location.ID), LogType.Information);

                bool isReadGeneralLog = false;
                //  ManualResetEvent wait = new ManualResetEvent(false);
                //Thread work = new Thread(new ThreadStart(() =>
                //{
                //    isReadGeneralLog = deviceClient.ReadGeneralLogData(1);
                //    wait.Set();
                //}));
                using var timeout = new CancellationTokenSource();
                timeout.CancelAfter(180 * 1000); // 3 minutes
                using var combinedTokenSource = CancellationTokenSource
                    .CreateLinkedTokenSource(token, timeout.Token);
                await Task.Run(() =>
                {
                    isReadGeneralLog = deviceClient.ReadGeneralLogData(1);
                }, combinedTokenSource.Token);

                // work.Start();
                //wait.WaitOne(180 * 1000);

                //if (work.IsAlive) work.Abort();

                if (isReadGeneralLog)
                {
                    logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Start ReadGeneralLogData");

                    while (deviceClient.SSR_GetGeneralLogData(1,
                        out string enrollNumber,
                        out int verifyMode,
                        out int inOutMode,
                        out int year, out int month, out int day,
                        out int hour, out int minute, out int second,
                        ref workCode))
                    {
                        var dateTime = new DateTime(year, month, day, hour, minute, second);
                        if (dateTime > deviceInfo.LastSyncTime)
                        {
                            list.Add(new AttendanceRecord(deviceInfo)
                            {
                                IsIn = inOutMode == 0 || inOutMode == 3,
                                //   IsBreak = inOutMode == 2 || inOutMode == 3,
                                UserId = enrollNumber,
                                Time = dateTime,
                                //IsAuto = true,
                                //IsDeleted = false
                            });

                            if (list.Count % 1000 == 0)
                                logger.LogWarning($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Counting '{list.Count}' attendance records");
                        }
                    }

                    logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Done SSR_GetGeneralLogData");
                }
                else
                {
                    ThrowLastError(deviceInfo, deviceClient, "ReadGeneralLogData", false);
                }

                //string serialNumber;

                //if (deviceClient.GetSerialNumber(1, out serialNumber))
                //{
                //    location.SerialNumber = serialNumber;
                //}

                //     logger(string.Format("{0} - Location({1}): Done ReadGeneralLogData", DateTime.Now.ToString(LOG_DATE_FORMAT), location.ID), LogType.Information);
                //     location.Enable(deviceClient);
                //     logger(string.Format("{0} - Location({1}): Enabled", DateTime.Now.ToString(LOG_DATE_FORMAT), location.ID), LogType.Information);
                deviceClient.Disconnect();
                logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Disconnected");
            }
            catch (Exception)
            {
                //logger(string.Format("{0} - Location({1}): Exception: {2}", DateTime.Now.ToString(LOG_DATE_FORMAT), location.ID, e.ToString()), LogType.Error);

                logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Location({deviceInfo.Name}): Try disconnect");

                if (isConnected)
                {
                    //location.Enable(deviceClient);
                    deviceClient.Disconnect();
                }

                logger.LogInformation($"{DateTime.Now.ToString(LOG_DATE_FORMAT)} - Device ({deviceInfo.Name}): Disconnected");

                throw;
            }

            return list;
        }
        private static bool ThrowLastError(DeviceInfo deviceInfo, CZKEM device, string operationName, bool ignoreErrorCode = true)
        {
            int errorCode = 0;
            device.GetLastError(ref errorCode);

            if (ignoreErrorCode || errorCode != 0)
                throw new Exception($"Error while doing '{operationName}' to deviceClient ({deviceInfo.Name}) with error code ({errorCode})");

            return false;
        }
    }
}