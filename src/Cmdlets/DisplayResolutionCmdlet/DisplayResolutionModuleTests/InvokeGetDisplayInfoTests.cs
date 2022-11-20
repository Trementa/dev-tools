using DisplayResolutionModuleTests.Utility;
using FluentAssertions;

namespace DisplayResolutionModuleTests;

public class InvokeGetDisplayInfoTests
{
    [Fact]
    public void Should_Get_At_Least_One_Display_Info()
    {
        using var powerShell = PSHostHelper.GetPowerShellHost();
        powerShell.AddCommand("Get-DisplayInfo");

        var results = powerShell.Invoke();
        results.Should().HaveCountGreaterThan(0);
    }
}
