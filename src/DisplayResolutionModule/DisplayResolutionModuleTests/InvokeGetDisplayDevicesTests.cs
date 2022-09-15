using DisplayResolutionModuleTests.Utility;
using FluentAssertions;

namespace DisplayResolutionModuleTests;

public class InvokeGetDisplayDevicesTests
{
    [Fact]
    public void Should_Get_At_Least_One_Display_Device()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DisplayDevice");

        var results = powerShell.Invoke();
        results.Should().HaveCountGreaterThan(0);
    }
}
