using OpenIPC_Config.Services;
using Prism.Events;

namespace OpenIPC_Config.Events;

public class TelemetryContentUpdatedEvent : PubSubEvent<TelemetryContentUpdatedMessage>
{
    
}

public class TelemetryContentUpdatedMessage
{
    public string Content { get; set; }

    public TelemetryContentUpdatedMessage(string content)
    {
        Content = content;
    }
}