using System.Diagnostics;

namespace DisplayResolutionModule.Management.PowerShell.Tests;

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