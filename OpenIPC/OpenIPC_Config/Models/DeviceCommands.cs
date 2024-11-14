namespace OpenIPC_Config.Models;

public static class DeviceCommands
{
    public const string WfbStartCommand = "wifibroadcast start";
    public const  string WfbStopCommand = "wifibroadcast stop";
    public const  string WfbRestartCommand = "wifibroadcast stop; sleep 2; wifibroadcast start";
    
    public const string MajesticRestartCommand = "killall -1 majestic";
    
    public const string TelemetryStartCommand = "telemetry start";
    public const string TelemetryStopCommand = "telemetry stop";
    public const  string TelemetryRestartCommand = "telemetry stop; sleep 2; telemetry start";

    public const string UART0OnCommand =
        "sed -i 's/console::respawn:\\/sbin\\/getty -L console 0 vt100/#console::respawn:\\/sbin\\/getty -L console 0 vt100/' /etc/inittab";

    public const string UART0OffCommand =
        "sed -i 's/#console::respawn:\\/sbin\\/getty -L console 0 vt100/console::respawn:\\/sbin\\/getty -L console 0 vt100/' /etc/inittab";

    public const string RebootCommand = "reboot";
    
    public const string FirmwareUpdateCommand = "sysupgrade -k -r -n --force_ver";
    
    public const string SendDroneKeyCommand = "reset";
    
    public const string ResetCameraCommand = "firstboot";
    
    public const string GetHostname = "hostname";

    public const string GenerateKeys = "wfb_keygen";
    
    public const string CopyGenerateKeys = "cp /root/gs.key /etc/";
    
    public static string BackUpGsKeysIfExist = "[ -f /root/gs.key ] && mv /root/gs.key \"/root/gs.key_$(date +'%m%d%Y')\"";
}