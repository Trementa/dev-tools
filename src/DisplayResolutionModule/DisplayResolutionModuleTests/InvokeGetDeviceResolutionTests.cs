using System.Collections.ObjectModel;
using System.Management.Automation;

namespace DisplayResolutionModule.Management.PowerShell.Tests;

public class InvokeGetDeviceResolutionTests
{
    [Fact]
    public void Current_Should_Give_Width_Height_Frequency()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DeviceResolution");
        powerShell.AddParameter("Current");

        dynamic results = powerShell.Invoke();
        var current = results[0];
        Assert.True(current.PelsWidth > 0 && current.PelsHeight > 0 && current.DisplayFrequency > 0);
    }

    [Fact]
    public void List_Should_Give_All_Viable_Resolutions()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DeviceResolution");
        powerShell.AddParameter("List");

        Collection<PSObject> list = powerShell.Invoke();
        Assert.True(list.Count > 0);
    }

    [Fact]
    public void History_Should_Be_Empty()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DeviceResolution");
        powerShell.AddParameter("History");

        Collection<PSObject> history = powerShell.Invoke();
        Assert.True(history.Count == 0);
    }
}
