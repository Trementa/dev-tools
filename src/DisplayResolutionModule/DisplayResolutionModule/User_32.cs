using System.Runtime.InteropServices;

namespace DisplayResolutionModule
{
    class User_32
    {
        [DllImport("user32.dll")]
        internal static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DeviceResolution devMode);
        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettings(ref DeviceResolution devMode, int flags);

        internal const int ENUM_CURRENT_SETTINGS = -1;
        internal const int CDS_UPDATEREGISTRY = 0x01;
        internal const int CDS_TEST = 0x02;
        internal const int DISP_CHANGE_SUCCESSFUL = 0;
        internal const int DISP_CHANGE_RESTART = 1;
        internal const int DISP_CHANGE_FAILED = -1;
    }
}
