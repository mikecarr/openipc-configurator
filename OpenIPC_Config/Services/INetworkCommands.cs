using System.Threading.Tasks;
using OpenIPC_Config.Models;

namespace OpenIPC_Config.Services;

public interface INetworkCommands
{
    extern Task<bool> Ping(DeviceConfig deviceConfig);

    Task Run(DeviceConfig deviceConfig, string command);
}