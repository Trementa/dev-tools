using System.Management.Automation;

namespace DisplayResolutionModule
{
    [Cmdlet(VerbsCommon.Set, nameof(DeviceResolution))]
    [OutputType(typeof(void))]
    public class SetDeviceResolutionCmdlet : PSCmdlet
    {
        readonly DeviceResolutionStack Stack;
        readonly ScreenResolutionManager ScreenResolutionManager;
        readonly string Push = nameof(Push);

        public SetDeviceResolutionCmdlet()
        {
            Stack = new DeviceResolutionStack(this);
            ScreenResolutionManager = new ScreenResolutionManager(this);
        }

        [Parameter(Mandatory = true, ParameterSetName = nameof(Push))]
        public int Width { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = nameof(Push))]
        public int Height { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = nameof(Push))]
        public int Frequency { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = nameof(Pop))]
        public SwitchParameter Pop { get; set; }

        protected override void ProcessRecord()
        {
            if (Pop)
            {
                if (Stack.TryProp(out DeviceResolution deviceResolution))
                {
                    //System.Diagnostics.Debug.Assert(deviceResolution.PelsWidth == 3160);
                    ScreenResolutionManager.SetResolution(deviceResolution);
                }
            }
            else
            {
                var resolutionResult = ScreenResolutionManager.ChangeResolution(Width, Height, Frequency);
                Stack.Push(resolutionResult.previousResolution);
            }
        }
    }
}
