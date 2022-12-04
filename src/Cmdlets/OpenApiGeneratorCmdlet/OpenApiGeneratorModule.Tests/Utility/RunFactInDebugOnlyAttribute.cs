using System.Diagnostics;

namespace OpenApiGeneratorModule.Tests.Utility;

public class RunFactInDebugOnlyAttribute : FactAttribute
{
    public RunFactInDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached)
            Skip = "Only running in interactive mode.";
    }
}