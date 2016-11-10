using CountlySDK;
using CountlySDK.Entities;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CountlyTests
{
    [TestFixture]
    public class iOSTests
    {
        private const string Server = "https://try.count.ly";
        private const string ApiKey = "<your-key-here>";

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

            Countly.IsLoggingEnabled = true;

            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            var segmentation = new Segmentation();
            segmentation.Add("test", new Random().Next(10).ToString());
            await Countly.RecordEvent("unit-test", 1, 0, segmentation);

            await Countly.EndSession();
        }
    }
}
