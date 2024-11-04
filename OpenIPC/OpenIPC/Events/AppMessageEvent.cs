using OpenIPC.Models;
using Prism.Events;

namespace OpenIPC.Events;

public class AppMessageEvent : PubSubEvent<AppMessage>
{
    
    
}

public class AppMessage
{
    public string Message { get; set; }
    
    public DeviceConfig DeviceConfig { get; set; }
    public WfbConfContentUpdatedMessage WfbConfContentUpdatedMessage { get; set; }

    public override string ToString()
    {
        return $"{nameof(Message)}: {Message}, {nameof(DeviceConfig)}: {DeviceConfig}";
    }
}

public class WfbConfContentUpdatedMessage
{
    public string Content { get; set; }

    public WfbConfContentUpdatedMessage(string content)
    {
        Content = content;
    }
}