using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenIPC_Config.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum DeviceType
{
    None,
    Camera,
    Radxa
}
// public enum DeviceType
// {
//     None,
//     Camera,
//     Radxa,
//     NVR
// }