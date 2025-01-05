namespace OpenIPC_Config.Models;

public static class DeviceCommands
{
    public const string Dos2UnixCommand = "dos2unix";
    
    public const string WfbStartCommand = "wifibroadcast start";
    public const string WfbStopCommand = "wifibroadcast stop";
    public const string WfbRestartCommand = "wifibroadcast stop; sleep 2; wifibroadcast start";
    
    public const string GsWfbStartCommand = "systemctl start openipc.service";
    public const string GsWfbStopCommand = "systemctl stop openipc.service";

    public const string MajesticRestartCommand = "killall -1 majestic";

    public const string TelemetryStartCommand = "telemetry start";
    public const string TelemetryStopCommand = "telemetry stop";
    public const string TelemetryRestartCommand = "telemetry stop; sleep 2; telemetry start";

    public const string UART0OnCommand =
        "sed -i 's/console::respawn:\\/sbin\\/getty -L console 0 vt100/#console::respawn:\\/sbin\\/getty -L console 0 vt100/' /etc/inittab";

    public const string UART0OffCommand =
        "sed -i 's/#console::respawn:\\/sbin\\/getty -L console 0 vt100/console::respawn:\\/sbin\\/getty -L console 0 vt100/' /etc/inittab";

    public const string RebootCommand = "reboot";

    public const string FirmwareUpdateCommand = "sysupgrade -k -r -n --force_ver";

    public const string SendDroneKeyCommand = "reset";

    public const string ResetCameraCommand = "firstboot";

    public const string GetHostname = "hostname -s";

    public const string GenerateKeys = "wfb_keygen";

    public const string CopyGenerateKeys = "cp /root/gs.key /etc/";

    public static string BackUpGsKeysIfExist =
        "[ -f /root/gs.key ] && mv /root/gs.key \"/root/gs.key_$(date +'%m%d%Y')\"";


    public static string MSPOSDExtraCommand =
        "sed -i 's/echo \\\"Starting wifibroadcast service...\\\"/echo \\\"Starting wifibroadcast service...\\\"\\n\\\t\\techo \"\\&L70 \\&F35 CPU:\\&C \\&B Temp:\\&T\\\" > \\/tmp\\/MSPOSD.msg /' /etc/init.d/S98datalink";
        // "sed -i 's/echo \\\"Starting wifibroadcast service...\\\"/echo \\\"\\&L70 \\&F35 CPU:\\&C \\&B Temp:\\&T\\\" > \\/tmp\\/MSPOSD.msg /' /etc/init.d/S98datalink";
        
    public static string DataLinkRestart = "/etc/init.d/S98datalink stop ;/etc/init.d/S98datalink start";
    
    
    public const string GSMSPDisplayportCommand = @"sed -i '/pixelpilot --osd --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4/c\pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4 ""&""' /config/scripts/stream.sh";
    public const string GSMSPDisplayport2Command = @"sed -i '/pixelpilot --osd --screen-mode $SCREEN_MODE/c\pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE ""&""' /config/scripts/stream.sh";
    
    public const string GsMavExtended1 =  @"sed -i '/pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4 ""&""/c\pixelpilot --osd --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4 --osd-telem-lvl 2 ""&""' /config/scripts/stream.sh";
    public const string GsMavExtended2 = @"sed -i '/pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE ""&""/c\pixelpilot --osd --screen-mode $SCREEN_MODE --osd-telem-lvl 2 ""&""' /config/scripts/stream.sh";

    public const string GsMavBasic1 = @"sed -i '/pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4 ""&""/c\pixelpilot --osd --screen-mode $SCREEN_MODE --dvr-framerate $REC_FPS --dvr-fmp4 --dvr record_${current_date}.mp4 --osd-telem-lvl 1 ""&""' /config/scripts/stream.sh";
    public const string GsMavBasic2 = @"sed -i '/pixelpilot --osd --osd-elements video,wfbng --screen-mode $SCREEN_MODE ""&""/c\pixelpilot --osd --screen-mode $SCREEN_MODE --osd-telem-lvl 1 ""&""' /config/scripts/stream.sh";

 


}