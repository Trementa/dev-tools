using System.Runtime.InteropServices;

namespace DisplayResolutionModule.PInvoke;
public static partial class User_32
{
    [DllImport("user32.dll")]
    internal static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DeviceResolution devMode);

    [StructLayout(LayoutKind.Sequential)]
    public record struct DeviceResolution
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        public short SpecVersion;
        public short DriverVersion;
        public short Size;
        public short DriverExtra;
        public int Fields;

        public short Orientation;
        public short PaperSize;
        public short PaperLength;
        public short PaperWidth;

        public short Scale;
        public short Copies;
        public short DefaultSource;
        public short PrintQuality;
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

        internal static DeviceResolution Create()
        {
            var dm = new DeviceResolution {
                DeviceName = new string(new char[32]),
                FormName = new string(new char[32])
            };
            dm.Size = (short)Marshal.SizeOf(dm);
            return dm;
        }
    };
}
