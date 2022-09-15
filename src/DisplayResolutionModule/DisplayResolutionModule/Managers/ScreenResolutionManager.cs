using System.Management.Automation;
using DisplayResolutionModule.PInvoke;

internal class ScreenResolutionManager : PSCmdletManagerBase
{
    public ScreenResolutionManager(PSCmdlet @base) : base(@base)
    { }

    internal User_32.DeviceResolution GetCurrentResolution()
    {
        var dm = User_32.DeviceResolution.Create();
        if (0 != User_32.EnumDisplaySettings(null, User_32.ENUM_CURRENT_SETTINGS, ref dm))
            return dm;

        return ThrowTerminatingError<User_32.DeviceResolution>("Failed to retrieve current resolution", 0);
    }

    internal IEnumerable<User_32.DeviceResolution> EnumDisplaySettings()
    {
        var dm = User_32.DeviceResolution.Create();
        for (int iModeNum = 0; 0 != User_32.EnumDisplaySettings(null, iModeNum, ref dm); iModeNum++)
            yield return dm with { };
    }

    internal User_32.DeviceResolution SetResolution(User_32.DeviceResolution deviceResolution)
    {
        int iRet = User_32.ChangeDisplaySettings(ref deviceResolution, User_32.CDS_TEST);

        if (iRet == User_32.DISP_CHANGE_FAILED)
            return ThrowTerminatingError<User_32.DeviceResolution>("Requested resolution could not be set", iRet);
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
                    return ThrowTerminatingError<User_32.DeviceResolution>("Failed to set requested resolution", iRet);
            }
        }
    }

    internal (User_32.DeviceResolution previousResolution, User_32.DeviceResolution newResolution) ChangeResolution(int width, int height, int freq)
    {
        var dm = User_32.DeviceResolution.Create();
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
                        return ThrowTerminatingError<(User_32.DeviceResolution, User_32.DeviceResolution)>("Failed to set requested resolution", iRet);
                }
            }
        }
        else
            return ThrowTerminatingError<(User_32.DeviceResolution, User_32.DeviceResolution)>("Failed to change resolution.", 0);
    }
}
