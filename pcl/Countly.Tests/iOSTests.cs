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
        private const string DeviceId = "39A9B658F49D474BB565DA868C54A6DF";

        [SetUp]
        public void SetUp()
        {
            //Sample data from our iOS app
            Device.AppVersion = "0.1.0.1";
            Device.Carrier = string.Empty;
            Device.DeviceId = DeviceId;
            Device.DeviceName = "iPhone";
            Device.Manufacturer = "Apple";
            Device.OS = "iOS";
            Device.OSVersion = "10.1";
            Device.Resolution = "640x1136";
            Device.OnlineCallback = () => false;
            Device.OrientationCallback = () => "Unknown";

            Countly.IsLoggingEnabled = true;
        }

        private Segmentation MakeSegment()
        {
            var segment = new Segmentation();
            segment.Add("Level", "1");
            segment.Add("Team", "Warriors");
            return segment;
        }

        [Test]
        public async Task RecordEvent()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            var segmentation = new Segmentation();
            segmentation.Add("test", new Random().Next(10).ToString());
            await Countly.RecordEvent("unit-test", 1, 0, 123.5, segmentation);

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

        [Test]
        public async Task SendViews()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";
            Countly.UserDetails.Custom["Level"] = "1";
            Countly.UserDetails.Custom["Team"] = "Warriors";

            await Countly.RecordView("ScreenA", MakeSegment());
            await Task.Delay(3500);
            await Countly.RecordView("ScreenB", MakeSegment());
            await Task.Delay(1242);
            await Countly.RecordView("ScreenC", MakeSegment());
            await Task.Delay(2322);
            await Countly.RecordView("ScreenD", MakeSegment());
            await Countly.RecordView("ScreenE", MakeSegment());

            await Countly.EndSession();
        }

        [Test]
        public async Task SendViewsWithStartEnd()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            await Countly.RecordView("ScreenA", MakeSegment());
            await Task.Delay(3500);
            await Countly.RecordView("ScreenB", MakeSegment());
            await Countly.EndSession();

            //Should still be on ScreenB
            await Countly.StartSession(Server, ApiKey);
            await Task.Delay(1242);
            await Countly.RecordView("ScreenC", MakeSegment());
            await Task.Delay(2322);
            await Countly.RecordView("ScreenD", MakeSegment());

            await Countly.EndSession();
        }

        [Test]
        public async Task SendAction()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            await Countly.RecordView("ScreenA", MakeSegment());

            var segmentation = MakeSegment();
            segmentation.Add("type", "click");
            segmentation.Add("x", "0");
            segmentation.Add("y", "0");
            segmentation.Add("width", "0");
            segmentation.Add("height", "0");
            await Countly.RecordEvent("[CLY]_action", 1, 0, segmentation);

            await Countly.EndSession();
        }

        [Test]
        public async Task SendBunchOfEvents()
        {
            await Countly.StartSession(Server, ApiKey);
            Countly.UserDetails.Name = "unit-test";

            await Countly.RecordEvent("ScreenA", 1, 0, MakeSegment());
            await Countly.RecordEvent("ScreenB", 1, 0, MakeSegment());
            await Countly.RecordEvent("ScreenC", 1, 0, MakeSegment());
            await Countly.RecordEvent("ScreenD", 1, 0, MakeSegment());
            await Countly.RecordEvent("ScreenE", 1, 0, MakeSegment());

            await Countly.EndSession();
        }
    }
}
