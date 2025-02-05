using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Renci.SshNet;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class GlobalViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isWfbYamlEnabled = false;
    
    
    public GlobalViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        ReadDevice();
    }

    public async void ReadDevice()
    {
        var cts = new CancellationTokenSource(10000); // 10 seconds
        var cancellationToken = cts.Token;
        
        if(DeviceConfig.Instance.DeviceType != DeviceType.None)
        {
            // check if wfb.yaml exists
            var cmdResult = await GetIsWfbYamlSupported(cancellationToken);

            IsWfbYamlEnabled = cmdResult.Result.ToString().Trim() == "true";
            Debug.WriteLine($"IsWfbYamlEnabled: {IsWfbYamlEnabled}");
            
        }
        
    }

    private async Task<SshCommand?> GetIsWfbYamlSupported(CancellationToken cancellationToken)
    {
        var command = "test -f /etc/wfb.yaml && echo 'true' || echo 'false'";
        var cmdResult = await SshClientService.ExecuteCommandWithResponseAsync(DeviceConfig.Instance, command, cancellationToken);

        return cmdResult;
    }
}