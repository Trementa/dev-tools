using System.IO;
using System.Management.Automation.Runspaces;

namespace OpenApiGeneratorModule.Tests.Utility;

public static class PSHostHelper
{
    public static System.Management.Automation.PowerShell GetPowerShellHost()
    {
        var iss = InitialSessionState.CreateDefault();
        var cd = Directory.GetCurrentDirectory();
        var path = Path.Combine(cd, $"{nameof(DisplayResolutionModule)}.dll");
        iss.ImportPSModule(new[] { path, "-Force" });
        var ps = System.Management.Automation.PowerShell.Create(iss);
        return ps;
    }
}
