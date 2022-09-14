using System.Management.Automation;

namespace DisplayResolutionModule
{
    class DeviceResolutionStack
    {
        const string DeviceResolutionStackName = nameof(DeviceResolutionStack);
        readonly PSCmdlet @base;

        public DeviceResolutionStack(PSCmdlet @base)
            => this.@base = @base;

        internal bool TryProp(out DeviceResolution deviceResolution)
        {
            var stack = GetStack();
            try
            {
                return stack.TryPop(out deviceResolution);
            }
            finally
            {
                SetStack(stack);
            }
        }

        internal void Push(DeviceResolution deviceResolution)
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

        internal IEnumerable<DeviceResolution> AsEnumerable()
            => GetStack().AsEnumerable();

        void SetStack(Stack<DeviceResolution> stack)
            => @base.SessionState.PSVariable.Set(DeviceResolutionStackName, stack);

        Stack<DeviceResolution> GetStack()
        {
            if (@base.SessionState.PSVariable.GetValue(DeviceResolutionStackName) is Stack<DeviceResolution> deviceResolutionStack)
                return deviceResolutionStack;
            else
                return new Stack<DeviceResolution>();
        }
    }
}
