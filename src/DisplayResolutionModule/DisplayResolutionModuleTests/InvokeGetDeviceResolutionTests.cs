using DisplayResolutionModuleTests.Utility;
using FluentAssertions;

namespace DisplayResolutionModuleTests;

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

        var list = powerShell.Invoke();
        list.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void History_Should_Be_Empty()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DeviceResolution");
        powerShell.AddParameter("History");

        var history = powerShell.Invoke();
        history.Should().HaveCount(0);
    }
}
