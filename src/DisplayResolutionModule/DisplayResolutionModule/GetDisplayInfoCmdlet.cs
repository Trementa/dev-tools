using System.Management.Automation;

namespace DisplayResolutionModule;
using Managers;
using static Managers.PInvoke.User_32;

[Cmdlet(VerbsCommon.Get, nameof(DisplayInfo))]
[OutputType(typeof(IEnumerable<DisplayInfo>))]
public class GetDisplayInfoCmdlet : PSCmdlet
{
    readonly DisplayDevicesManager DisplayDevicesManager;

    public GetDisplayInfoCmdlet() =>
        DisplayDevicesManager = new DisplayDevicesManager(this);

    [Parameter(Position = 0)]
    [PSDefaultValue(Value = DisplayDeviceStateFlags.None)]
    public DisplayDeviceStateFlags DisplayDeviceStateFlags { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(DisplayDevicesManager.EnumDisplayMonitors(DisplayDeviceStateFlags), true);
    }
}
