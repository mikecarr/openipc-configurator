using System.Net.NetworkInformation;
using System.Threading.Tasks;
using OpenIPC.Models;
using Serilog;

namespace OpenIPC.Services;

public class NetworkCommands : INetworkCommands
{
    // Create a new Ping instance
    private Ping ping;
    
    public NetworkCommands()
    {
        ping = new Ping();
    }
    
    public Task<bool> Ping(string ipAddress)
    {
        
        // Send a ping request
        PingReply reply = ping.Send(ipAddress);

        // Check the status of the ping request
        if (reply.Status == IPStatus.Success)
        {
            Log.Debug($"Ping successful: {reply.Status}");
            return Task.FromResult(true);
        }
        else
        {
            Log.Debug($"Ping failed: {reply.Status}");
            return Task.FromResult(false);
        }
    }

    public Task Run(DeviceConfig deviceConfig, string command)
    {
        throw new System.NotImplementedException();
    }
}