using System.Management.Automation;

namespace DisplayResolutionModule.Managers;
public abstract class PSCmdletManagerBase
{
    readonly PSCmdlet @base;
    protected PSCmdletManagerBase(PSCmdlet @base)
        => this.@base = @base;

    protected T ThrowTerminatingError<T>(string error, int iRet)
    {
        var errorRecord = new ErrorRecord(new Exception(error), iRet.ToString(), ErrorCategory.DeviceError, this);
        @base.ThrowTerminatingError(errorRecord);
        return default;
    }

    protected void WriteError(string error)
    {
        @base.WriteError(new ErrorRecord(new Exception(), error, ErrorCategory.DeviceError, this));
    }
}
