using OpenIPC_Config.Models;
using Prism.Events;

namespace OpenIPC_Config.Events;

public class DeviceTypeChangeEvent : PubSubEvent<DeviceType>
{
}