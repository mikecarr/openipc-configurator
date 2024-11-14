using System;
using System.Collections.Generic;
using OpenIPC_Config.Models;
using Prism.Events;

namespace OpenIPC_Config.Events;

public class DeviceContentUpdateEvent : PubSubEvent<DeviceContentUpdatedMessage>
{

}

public class DeviceContentUpdatedMessage
{
    // Dictionary to store content as key-value pairs
    
    
    //public Dictionary<string, string> DeviceContent { get; set; } = new Dictionary<string, string>();
    
    public DeviceConfig DeviceConfig { get; set; } = new DeviceConfig();
    public string WifiBroadcastContent { get; set; } = String.Empty;
    public string ScreenModeContent { get; set; } = String.Empty;
    public string WfbConfContent { get; set; } = String.Empty;
    
    public string MajesticContent { get; set; } = String.Empty;
    
    public string TelemetryContent { get; set; } = String.Empty;


    

    public override string ToString()
    {
        return
            $"{nameof(DeviceConfig)}: {DeviceConfig}, {nameof(WifiBroadcastContent)}: {WifiBroadcastContent}, " +
            $"{nameof(ScreenModeContent)}: {ScreenModeContent}, {nameof(WfbConfContent)}: {WfbConfContent}, " +
            $"{nameof(MajesticContent)}: {MajesticContent}, {nameof(TelemetryContent)}: {TelemetryContent}";
    }
}



