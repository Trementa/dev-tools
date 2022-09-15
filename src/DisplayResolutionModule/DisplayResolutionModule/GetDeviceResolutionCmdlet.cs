using System.Management.Automation;
using static DisplayResolutionModule.PInvoke.User_32;

namespace DisplayResolutionModule
{
    [Cmdlet(VerbsCommon.Get, nameof(DeviceResolution))]
    [OutputType(typeof(DeviceResolution), ParameterSetName = new[] { nameof(Current) })]
    [OutputType(typeof(IEnumerable<DeviceResolution>), ParameterSetName = new[] { nameof(List), nameof(History) })]
    public class GetDeviceResolutionCmdlet : PSCmdlet
    {
        readonly DeviceResolutionStack Stack;
        readonly ScreenResolutionManager ScreenResolutionManager;

        public GetDeviceResolutionCmdlet() =>
            (Stack, ScreenResolutionManager) = (new DeviceResolutionStack(this), new ScreenResolutionManager(this));

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
}
