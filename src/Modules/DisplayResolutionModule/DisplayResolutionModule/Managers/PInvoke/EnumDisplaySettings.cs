using System.Runtime.InteropServices;

namespace DisplayResolutionModule.Managers.PInvoke ;
public static partial class User_32
{
    [DllImport("user32.dll")]
    internal static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DisplayDeviceResolution devMode);

    [StructLayout(LayoutKind.Sequential)]
    public record struct DisplayDeviceResolution
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        public short SpecVersion;
        public short DriverVersion;
        public short Size;
        public short DriverExtra;
        public int Fields;

        public POINTL Position;
        public int DisplayOrientation;
        public int DisplayFixedOutput;

        public short Color;
        public short Duplex;
        public short YResolution;
        public short TTOption;
        public short Collate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FormName;
        public short LogPixels;
        public short BitsPerPel;
        public int PelsWidth;
        public int PelsHeight;

        public int DisplayFlags;
        public int DisplayFrequency;

        public int ICMMethod;
        public int ICMIntent;
        public int MediaType;
        public int DitherType;
        public int Reserved1;
        public int Reserved2;

        public int PanningWidth;
        public int PanningHeight;

        [StructLayout(LayoutKind.Sequential)]
        public record struct POINTL
        {
            int X;
            int Y;
        }

        internal static DisplayDeviceResolution Create()
        {
            var dm = new DisplayDeviceResolution {
                DeviceName = new string(new char[32]),
                FormName = new string(new char[32])
            };
            dm.Size = (short)Marshal.SizeOf(dm);
            return dm;
        }
    };
}
