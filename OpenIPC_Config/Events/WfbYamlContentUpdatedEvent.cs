using Prism.Events;

namespace OpenIPC_Config.Events;

public class WfbYamlContentUpdatedEvent : PubSubEvent<WfbYamlContentUpdatedMessage>
{
}

public class WfbYamlContentUpdatedMessage
{
    public WfbYamlContentUpdatedMessage(string content)
    {
        Content = content;
    }

    public string Content { get; set; }
}