using System.Collections;
using System.Runtime.InteropServices;

namespace DisplayResolutionModule
{
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
    };
}
