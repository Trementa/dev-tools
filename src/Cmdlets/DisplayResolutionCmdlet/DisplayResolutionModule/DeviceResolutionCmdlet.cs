using System.Management.Automation;

namespace DisplayResolutionModule;

public abstract class DeviceResolutionCmdlet : PSCmdlet
{
    protected readonly DeviceResolutionStack Stack;
    protected DeviceResolutionCmdlet()
        => Stack = new DeviceResolutionStack(this);
}
