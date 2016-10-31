using CountlySDK.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countly.Tests
{
    [TestFixture]
    public class iOSTests
    {
        [Test]
        public async Task BeginSession()
        {
            //Sample data from our iOS app
            Device.AppVersion = "0.1.0.1";
            Device.Carrier = string.Empty;
            Device.DeviceId = Guid.NewGuid().ToString().ToUpperInvariant();
            Device.DeviceName = "iPhone";
            Device.Manufacturer = "Apple";
            Device.OS = "iOS";
            Device.OSVersion = "10.1";
            Device.Resolution = "640x1136";
            Device.OnlineCallback = () => false;
            Device.OrientationCallback = () => "Unknown";

            await CountlySDK.Countly.StartSession("https://try.count.ly", "*****");
        }
    }
}
