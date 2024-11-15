using Prism.Events;

namespace OpenIPC_Config.Events;

public class WfbConfContentUpdatedEvent : PubSubEvent<WfbConfContentUpdatedMessage>
{
}

public class WfbConfContentUpdatedMessage
{
    public WfbConfContentUpdatedMessage(string content)
    {
        Content = content;
    }

    public string Content { get; set; }
}