using System.Diagnostics;

namespace DisplayResolutionModuleTests.Utility;

public class RunFactInDebugOnlyAttribute : FactAttribute
{
    public RunFactInDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Only running in interactive mode.";
        }
    }
}