using System.Management.Automation;

namespace DisplayResolutionModule;
using static Managers.PInvoke.User_32;

public class DeviceResolutionStack
{
    const string DeviceResolutionStackName = nameof(DeviceResolutionStack);
    readonly PSCmdlet @base;

    public DeviceResolutionStack(PSCmdlet @base)
        => this.@base = @base;

    internal bool TryProp(out DisplayDeviceResolution deviceResolution)
    {
        var stack = GetStack();
        try
        {
            if (stack.Count == 0)
            {
                deviceResolution = default;
                return false;
            }

            deviceResolution = stack.Pop();
            return true;
        }
        finally
        {
            SetStack(stack);
        }
    }

    internal void Push(DisplayDeviceResolution deviceResolution)
    {
        var stack = GetStack();
        try
        {
            stack.Push(deviceResolution);
        }
        finally
        {
            SetStack(stack);
        }
    }

    internal IEnumerable<DisplayDeviceResolution> AsEnumerable()
        => GetStack().AsEnumerable();

    void SetStack(Stack<DisplayDeviceResolution> stack)
        => @base.SessionState.PSVariable.Set(DeviceResolutionStackName, stack);

    Stack<DisplayDeviceResolution> GetStack()
    {
        if (@base.SessionState.PSVariable.GetValue(DeviceResolutionStackName) is Stack<DisplayDeviceResolution> deviceResolutionStack)
            return deviceResolutionStack;
        else
            return new Stack<DisplayDeviceResolution>();
    }
}
