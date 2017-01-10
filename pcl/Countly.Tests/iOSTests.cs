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
        private const string DeviceId = "39A9B658F49D474BB565DA868C54A6DE";
        private const string UserName = "Donald Trump";

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
            Countly.UserDetails.Name = UserName;
            await Countly.StartSession(Server, ApiKey);

            var segmentation = new Segmentation();
            segmentation.Add("test", new Random().Next(10).ToString());
            Countly.RecordEvent("unit-test", 1, 0, 123.5, segmentation);

            await Countly.EndSession();
        }

        [Test]
        public async Task SendException()
        {
            Countly.UserDetails.Name = UserName;
            await Countly.StartSession(Server, ApiKey);

            await Countly.RecordException("Oh no!", "this is a trace\nmorelines\n");

            await Countly.EndSession();
        }

        [Test]
        public async Task SendViews()
        {
            Countly.UserDetails.Name = UserName;
            Countly.UserDetails.Custom["Level"] = "1";
            Countly.UserDetails.Custom["Team"] = "Warriors";
            await Countly.StartSession(Server, ApiKey);

            Countly.RecordView("ScreenA", MakeSegment());
            await Task.Delay(3500);
            Countly.RecordView("ScreenB", MakeSegment());
            await Task.Delay(1242);
            Countly.RecordView("ScreenC", MakeSegment());
            await Task.Delay(2322);
            Countly.RecordView("ScreenD", MakeSegment());
            Countly.RecordView("ScreenE", MakeSegment());

            await Countly.EndSession();
        }

        [Test]
        public async Task SendViewsWithStartEnd()
        {
            Countly.UserDetails.Name = UserName;
            await Countly.StartSession(Server, ApiKey);

            Countly.RecordView("ScreenA", MakeSegment());
            await Task.Delay(3500);
            Countly.RecordView("ScreenB", MakeSegment());
            await Countly.EndSession();

            //Should still be on ScreenB
            await Countly.StartSession(Server, ApiKey);
            await Task.Delay(1242);
            Countly.RecordView("ScreenC", MakeSegment());
            await Task.Delay(2322);
            Countly.RecordView("ScreenD", MakeSegment());

            await Countly.EndSession();
        }

        [Test]
        public async Task SendAction()
        {
            Countly.UserDetails.Name = UserName;
            await Countly.StartSession(Server, ApiKey);

            Countly.RecordView("ScreenA", MakeSegment());

            var segmentation = MakeSegment();
            segmentation.Add("type", "click");
            segmentation.Add("x", "0");
            segmentation.Add("y", "0");
            segmentation.Add("width", "0");
            segmentation.Add("height", "0");
            Countly.RecordEvent("[CLY]_action", segmentation: segmentation);

            await Countly.EndSession();
        }

        [Test]
        public async Task SendBunchOfEvents()
        {
            Countly.UserDetails.Name = UserName;
            await Countly.StartSession(Server, ApiKey);

            Countly.RecordEvent("ActionA", segmentation: MakeSegment());
            await Task.Delay(25);
            Countly.RecordEvent("ActionB", segmentation: MakeSegment());
            await Task.Delay(25);
            Countly.RecordEvent("ActionC", segmentation: MakeSegment());
            await Task.Delay(2100);
            Countly.RecordEvent("ActionD", segmentation: MakeSegment());
            await Task.Delay(25);
            Countly.RecordEvent("ActionE", segmentation: MakeSegment());
            await Task.Delay(1000);

            await Countly.EndSession();
        }
    }
}
