using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountlySDK.Entities
{
    /// <summary>
    /// This class provides several static methods to retrieve information about the current device and operating environment.
    /// NOTE: PCL implementation requires callbacks or direct setting of properties
    /// </summary>
    public static class Device
    {
        /// <summary>
        /// Returns the unique device identificator
        /// </summary>
        public static string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the display name of the current operating system
        /// </summary>
        public static string OS
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current operating system version as a displayable string
        /// </summary>
        public static string OSVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current device manufacturer
        /// </summary>
        public static string Manufacturer
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current device model
        /// </summary>
        public static string DeviceName
        {
            get;
            set;
        }

        /// <summary>
        /// Returns application version from Package.appxmanifest
        /// </summary>
        public static string AppVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Returns device resolution in <width_px>x<height_px> format
        /// </summary>
        public static string Resolution
        {
            get;
            set;
        }

        /// <summary>
        /// Returns cellular mobile operator
        /// </summary>
        public static string Carrier
        {
            get;
            set;
        }

        /// <summary>
        /// Set this callback to provide orientation logic
        /// </summary>
        public static Func<string> OrientationCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Returns current device orientation
        /// </summary>
        public static string Orientation
        {
            get { return OrientationCallback?.Invoke(); }
        }

        /// <summary>
        /// Set this callback to provide online logic
        /// </summary>
        public static Func<bool> OnlineCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Returns current device connection to the internet
        /// </summary>
        public static bool Online
        {
            get { return OnlineCallback?.Invoke() ?? true; }
        }
    }
}
