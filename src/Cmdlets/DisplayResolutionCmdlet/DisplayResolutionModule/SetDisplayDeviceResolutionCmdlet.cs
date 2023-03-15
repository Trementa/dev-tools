using System.Management.Automation;

namespace DisplayResolutionModule;
using Managers;
using static Managers.PInvoke.User_32;

[Cmdlet(VerbsCommon.Set, nameof(DisplayDeviceResolution))]
[OutputType(typeof(void))]
public class SetDisplayDeviceResolutionCmdlet : DeviceResolutionCmdlet
{
    readonly ScreenResolutionManager ScreenResolutionManager;
    readonly string Push = nameof(Push);

    public SetDisplayDeviceResolutionCmdlet()
        => ScreenResolutionManager = new ScreenResolutionManager(this);

    [Parameter(Mandatory = true, Position = 0, ParameterSetName = nameof(Push))]
    public int Width { get; set; }
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = nameof(Push))]
    public int Height { get; set; }
    [Parameter(Mandatory = true, Position = 2, ParameterSetName = nameof(Push))]
    public int Frequency { get; set; }

    [Parameter(Mandatory = true, Position = 3, ParameterSetName = nameof(Pop))]
    public SwitchParameter Pop { get; set; }

    protected override void ProcessRecord()
    {
        if (Pop)
        {
            if (Stack.TryProp(out DisplayDeviceResolution deviceResolution))
                ScreenResolutionManager.SetResolution(deviceResolution);
        }
        else
        {
            var resolutionResult = ScreenResolutionManager.ChangeResolution(Width, Height, Frequency);
            Stack.Push(resolutionResult.previousResolution);
        }
    }
}
