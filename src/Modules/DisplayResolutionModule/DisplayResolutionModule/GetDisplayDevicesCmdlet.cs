using System.Management.Automation;

namespace DisplayResolutionModule;
using Managers;
using static Managers.PInvoke.User_32;

[Cmdlet(VerbsCommon.Get, nameof(DisplayDevice))]
[OutputType(typeof(IEnumerable<DisplayDevice>))]
public class GetDisplayDevicesCmdlet : PSCmdlet
{
    readonly DisplayDevicesManager DisplayDevicesManager;

    public GetDisplayDevicesCmdlet() =>
        DisplayDevicesManager = new DisplayDevicesManager(this);

    [Parameter(Position = 0)]
    [PSDefaultValue(Value = DisplayDeviceStateFlags.None)]
    public DisplayDeviceStateFlags DisplayDeviceStateFlags { get; set; }

    protected override void ProcessRecord()
    {
        WriteObject(DisplayDevicesManager.EnumDisplayDevices(DisplayDeviceStateFlags), true);
    }
}
