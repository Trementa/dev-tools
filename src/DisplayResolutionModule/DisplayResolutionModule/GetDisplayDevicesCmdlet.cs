using System.Management.Automation;
using static DisplayResolutionModule.PInvoke.User_32;

namespace DisplayResolutionModule
{
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
            WriteObject(DisplayDevicesManager.EnumDisplayDevices(DisplayDeviceStateFlags));
        }
    }
}
