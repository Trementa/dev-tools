using System.Management.Automation;

namespace DisplayResolutionModule;
using Managers;
using static Managers.PInvoke.User_32;

[Cmdlet(VerbsCommon.Get, nameof(DisplayDeviceResolution))]
[OutputType(typeof(DisplayDeviceResolution), ParameterSetName = new[] { nameof(Current) })]
[OutputType(typeof(IEnumerable<DisplayDeviceResolution>), ParameterSetName = new[] { nameof(List), nameof(History) })]
public class GetDeviceResolutionCmdlet : DeviceResolutionCmdlet
{
    readonly ScreenResolutionManager ScreenResolutionManager;
    public GetDeviceResolutionCmdlet() 
        => ScreenResolutionManager = new ScreenResolutionManager(this);

    [Parameter(Mandatory = true, ParameterSetName = nameof(Current))]
    public SwitchParameter Current { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = nameof(List))]
    public SwitchParameter List { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = nameof(History))]
    public SwitchParameter History { get; set; }

    protected override void ProcessRecord()
    {
        if (Current)
            WriteObject(ScreenResolutionManager.GetCurrentResolution());

        if (List)
            WriteObject(ScreenResolutionManager.EnumDisplaySettings(), true);

        if(History)
            WriteObject(Stack.AsEnumerable(), true);
    }
}
