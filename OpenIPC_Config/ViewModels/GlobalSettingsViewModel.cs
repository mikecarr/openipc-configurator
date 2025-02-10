using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Renci.SshNet;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class GlobalSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isWfbYamlEnabled = false;
    
    [ObservableProperty]
    private bool _isMobile = false;
    
    
    public GlobalSettingsViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        ReadDevice();
    }

    public async Task ReadDevice()
    {
        var cts = new CancellationTokenSource(30000); // 10 seconds

        var cancellationToken = cts.Token;
        
        if(DeviceConfig.Instance.DeviceType != DeviceType.None)
        {
            try
            {
                // check if wfb.yaml exists
                var cmdResult = await GetIsWfbYamlSupported(cts.Token);

                //TODO: check if wfb.yaml exists when all parameters are supported
                // https://github.com/svpcom/wfb-ng/wiki/Drone-auto-provisioning

                //IsWfbYamlEnabled = cmdResult.Result.ToString().Trim() == "true";
                IsWfbYamlEnabled = false;
                Debug.WriteLine($"IsWfbYamlEnabled: {IsWfbYamlEnabled}");
            }
            catch (Exception e)
            {
                Logger.Error("Error checking if wfb.yaml exists: " + e.Message);

            }
            finally
            {
                cts.Cancel();
            }
            
            
        }
        
    }

    private async Task<SshCommand?> GetIsWfbYamlSupported(CancellationToken cancellationToken)
    {
        var command = "test -f /etc/wfb.yaml && echo 'true' || echo 'false'";
        var cmdResult = await SshClientService.ExecuteCommandWithResponseAsync(DeviceConfig.Instance, command, cancellationToken);

        return cmdResult;
    }
}