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

        [SetUp]
        public void SetUp()
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
        }

        [Test]
        public async Task RecordEvent()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            var segmentation = new Segmentation();
            segmentation.Add("test", new Random().Next(10).ToString());
            await Countly.RecordEvent("unit-test", 1, 0, segmentation);

            await Countly.EndSession();
        }

        [Test]
        public async Task SendException()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            await Countly.RecordException("Oh no!", "this is a trace\nmorelines\n");

            await Countly.EndSession();
        }
    }
}
