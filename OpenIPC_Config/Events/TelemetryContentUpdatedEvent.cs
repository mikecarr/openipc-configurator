using Prism.Events;

namespace OpenIPC_Config.Events;

public class TelemetryContentUpdatedEvent : PubSubEvent<TelemetryContentUpdatedMessage>
{
}

public class TelemetryContentUpdatedMessage
{
    public TelemetryContentUpdatedMessage(string content)
    {
        Content = content;
    }

    public string Content { get; set; }
}