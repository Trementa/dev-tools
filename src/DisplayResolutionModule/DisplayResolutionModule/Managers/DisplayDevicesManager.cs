using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Runtime.InteropServices;
using DisplayResolutionModule.PInvoke;

internal class DisplayDevicesManager : PSCmdletManagerBase
{
    public DisplayDevicesManager(PSCmdlet @base) : base(@base)
    { }

    internal IEnumerable<User_32.DisplayDevice> EnumDisplayDevices(User_32.DisplayDeviceStateFlags displayDeviceStateFlags)
    {
        var dd = User_32.DisplayDevice.Create();
        for (uint iDevNum = 0; User_32.EnumDisplayDevices(null, iDevNum, ref dd, displayDeviceStateFlags); iDevNum++)
            yield return dd with { };
    }

    internal IEnumerable<User_32.DisplayInfo> EnumDisplayMonitors(User_32.DisplayDeviceStateFlags displayDeviceStateFlags)
    {
        var col = new Collection<User_32.DisplayInfo>();
        User_32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref User_32.Rect lprcMonitor, IntPtr dwData)
            {
                var mi = User_32.MonitorInfoEx.Create();
                mi.Size = Marshal.SizeOf(mi);
                if (User_32.GetMonitorInfo(hMonitor, ref mi))
                {
                    User_32.DisplayInfo di = new User_32.DisplayInfo {
                        ScreenWidth = (mi.Monitor.Right - mi.Monitor.Left).ToString(),
                        ScreenHeight = (mi.Monitor.Bottom - mi.Monitor.Top).ToString(),
                        MonitorArea = mi.Monitor,
                        WorkArea = mi.WorkArea,
                        Availability = mi.Flags.ToString()
                    };
                    col.Add(di);
                }
                return true;


            }, IntPtr.Zero);

        return col;
    }
}
