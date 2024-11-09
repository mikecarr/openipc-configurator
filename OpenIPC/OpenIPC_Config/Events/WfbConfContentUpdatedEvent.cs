using OpenIPC_Config.Services;
using Prism.Events;

namespace OpenIPC_Config.Events;

public class WfbConfContentUpdatedEvent : PubSubEvent<WfbConfContentUpdatedMessage>
{
    
}

public class WfbConfContentUpdatedMessage
{
    public string Content { get; set; }

    public WfbConfContentUpdatedMessage(string content)
    {
        Content = content;
    }
}