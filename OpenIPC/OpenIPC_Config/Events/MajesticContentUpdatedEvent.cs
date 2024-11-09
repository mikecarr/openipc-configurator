using OpenIPC_Config.Services;
using Prism.Events;

namespace OpenIPC_Config.Events;

public class MajesticContentUpdatedEvent : PubSubEvent<MajesticContentUpdatedMessage>
{
    
}

public class MajesticContentUpdatedMessage
{
    public string Content { get; set; }

    public MajesticContentUpdatedMessage(string content)
    {
        Content = content;
    }
}