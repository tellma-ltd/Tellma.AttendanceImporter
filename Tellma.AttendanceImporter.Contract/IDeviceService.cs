using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.AttendanceImporter.Contract
{
    public interface IDeviceService
    {
        public string DeviceType { get;}
        Task<IEnumerable<AttendanceRecord>> LoadFromDevice(DeviceInfo info, CancellationToken token);   
    }
}