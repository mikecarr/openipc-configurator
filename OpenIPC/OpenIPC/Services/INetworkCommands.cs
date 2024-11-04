using System.Threading.Tasks;
using OpenIPC.Models;

namespace OpenIPC.Services;

public interface INetworkCommands
{
    extern Task<bool> Ping(DeviceConfig deviceConfig);
    
    Task Run(DeviceConfig deviceConfig, string command);
}