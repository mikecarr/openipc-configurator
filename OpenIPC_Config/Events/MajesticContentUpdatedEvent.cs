using Prism.Events;

namespace OpenIPC_Config.Events;

public class MajesticContentUpdatedEvent : PubSubEvent<MajesticContentUpdatedMessage>
{
}

public class MajesticContentUpdatedMessage
{
    public MajesticContentUpdatedMessage(string content)
    {
        Content = content;
    }

    public string Content { get; set; }
}