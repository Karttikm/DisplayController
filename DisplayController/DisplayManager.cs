using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace DisplayController
{
    public static class DisplayManager
    {
        static int originalWidth;
        static int originalHeight;
        static int originalDpi;

        public static void SaveCurrentDisplaySettings()
        {
            var dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)); // MUST set before calling EnumDisplaySettings
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                originalWidth = dm.dmPelsWidth;
                originalHeight = dm.dmPelsHeight;
                originalDpi = GetScaling();
            }
            else
            {
                throw new InvalidOperationException("EnumDisplaySettings failed");
            }
        }

        public static void SetDisplayResolution(int width, int height)
        {
            var dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmPelsWidth = width;
            dm.dmPelsHeight = height;
            dm.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT;
            ChangeDisplaySettings(ref dm, 0);
        }

        public static void RevertDisplaySettings()
        {
            SetDisplayResolution(originalWidth, originalHeight);
            SetDisplayScaling(originalDpi);
        }

        public static void SetDisplayScaling(int scalingPercent)
        {
            int dpi = scalingPercent switch
            {
                100 => 96,
                125 => 120,
                150 => 144,
                175 => 168,
                200 => 192,
                _ => 96
            };
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "LogPixels", dpi, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "Win8DpiScaling", 1, RegistryValueKind.DWord);
        }

        public static int GetScaling()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\PerMonitorSettings");
            if(key == null) return 100;
            foreach(var monitor in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(monitor);
                if(subKey == null) continue;
                var dpiValue = subKey.GetValue("DpiValue");
                if (dpiValue == null) continue;
                int dpiIndex = (int)dpiValue;
                return dpiIndex switch
                {
                    0 => 100,
                    1 => 125,
                    2 => 150,
                    3 => 175,
                    4 => 200,
                    _ => 100
                };
            }
            return 100;
        }

        #region Win32
        const int ENUM_CURRENT_SETTINGS = -1;
        const int DM_PELSWIDTH = 0x80000;
        const int DM_PELSHEIGHT = 0x100000;

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        #endregion
    }
}
