using System.Management.Automation;
using System.Runtime.InteropServices;
using DisplayResolutionModule;

internal class ScreenResolutionManager
{
    readonly PSCmdlet @base;
    public ScreenResolutionManager(PSCmdlet @base)
        => this.@base = @base;

    T ThrowTerminatingError<T>(string error, int iRet)
    {
        var errorRecord = new ErrorRecord(new Exception("Failed to retrieve current resolution"), "1", ErrorCategory.DeviceError, this);
        @base.ThrowTerminatingError(errorRecord);
        return default;
    }

    void WriteError(string error)
    {
        @base.WriteError(new ErrorRecord(new Exception(), error, ErrorCategory.DeviceError, this));
    }

    internal DeviceResolution GetCurrentResolution()
    {
        DeviceResolution dm = GetDevMode1();
        if (0 != User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
            return dm;

        return ThrowTerminatingError<DeviceResolution>("Failed to retrieve current resolution", 0);
    }

    internal IEnumerable<DeviceResolution> EnumDisplaySettings()
    {
        DeviceResolution dm = GetDevMode1();
        for (int iModeNum = 0; 0 != User_32.EnumDisplaySettings(null, iModeNum, ref dm); iModeNum++)
            yield return dm with { };
    }

    internal DeviceResolution SetResolution(DeviceResolution deviceResolution)
    {
        int iRet = User_32.ChangeDisplaySettings(ref deviceResolution, User_32.CDS_TEST);

        if (iRet == User_32.DISP_CHANGE_FAILED)
            return ThrowTerminatingError<DeviceResolution>("Requested resolution could not be set", iRet);
        else
        {
            iRet = User_32.ChangeDisplaySettings(ref deviceResolution, User_32.CDS_UPDATEREGISTRY);
            switch (iRet)
            {
                case User_32.DISP_CHANGE_SUCCESSFUL:
                    return deviceResolution;
                case User_32.DISP_CHANGE_RESTART:
                    WriteError("You need to reboot for the change to happen.\n If you feel any problems after rebooting your machine\nThen try to change resolution in Safe Mode.");
                    return deviceResolution;
                default:
                    return ThrowTerminatingError<DeviceResolution>("Failed to set requested resolution", iRet);
            }
        }
    }

    internal (DeviceResolution previousResolution, DeviceResolution newResolution) ChangeResolution(int width, int height, int freq)
    {
        DeviceResolution dm = GetDevMode1();
        if (0 != User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
        {
            var dmNew = dm with { PelsWidth = width, PelsHeight = height, DisplayFrequency = freq };
            int iRet = User_32.ChangeDisplaySettings(ref dmNew, User_32.CDS_TEST);

            if (iRet == User_32.DISP_CHANGE_FAILED)
                throw new Exception($"Requested resolution could not be set. Error code: {iRet}");
            else
            {
                iRet = User_32.ChangeDisplaySettings(ref dmNew, User_32.CDS_UPDATEREGISTRY);
                switch (iRet)
                {
                    case User_32.DISP_CHANGE_SUCCESSFUL:
                        return (dm, dmNew);
                    case User_32.DISP_CHANGE_RESTART:
                        WriteError("You need to reboot for the change to happen.\n If you feel any problems after rebooting your machine\nThen try to change resolution in Safe Mode.");
                        return (dm, dmNew);
                    default:
                        return ThrowTerminatingError<(DeviceResolution, DeviceResolution)>("Failed to set requested resolution", iRet);
                }
            }
        }
        else
            return ThrowTerminatingError<(DeviceResolution, DeviceResolution)>("Failed to change resolution.", 0);
    }

    static DeviceResolution GetDevMode1()
    {
        DeviceResolution dm = new DeviceResolution();
        dm.DeviceName = new String(new char[32]);
        dm.FormName = new String(new char[32]);
        dm.Size = (short)Marshal.SizeOf(dm);
        return dm;
    }
}
