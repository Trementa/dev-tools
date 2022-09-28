using System.Diagnostics;
using System.Reflection;

namespace GK.WebLib.Extensions
{
    public static partial class AssemblyMetadataInformation
    {
        public static string InformationalVersion =>
            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
        public static string AssemblyName => Assembly.GetEntryAssembly().GetName().Name;
        public static string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}
