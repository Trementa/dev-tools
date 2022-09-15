using DisplayResolutionModuleTests.Utility;

namespace DisplayResolutionModuleTests;

public class InvokeSetDeviceResolutionTests
{
    [RunFactInDebugOnly(Timeout = 10000)]
    public void Change_Width_Height_Frequency_And_Restore()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell
                .AddCommand("Set-DeviceResolution")
                .AddParameter("Width", 1920)
                .AddParameter("Height", 1080)
                .AddParameter("Frequency", 60)
                .AddScript("Start-Sleep -Seconds 5")
                .AddStatement()
                .AddCommand("Set-DeviceResolution")
                .AddParameter("Pop")
                .Invoke();
    }
}
