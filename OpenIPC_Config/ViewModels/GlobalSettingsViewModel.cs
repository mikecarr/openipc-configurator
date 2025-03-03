using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Renci.SshNet;
using Serilog;

namespace OpenIPC_Config.ViewModels;

/// <summary>
/// ViewModel for managing global application settings and device configuration
/// </summary>
public partial class GlobalSettingsViewModel : ViewModelBase
{
    #region Observable Properties
    /// <summary>
    /// Gets or sets whether WFB YAML configuration is enabled
    /// </summary>
    [ObservableProperty]
    private bool _isWfbYamlEnabled = false;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of GlobalSettingsViewModel
    /// </summary>
    public GlobalSettingsViewModel(
        ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        ReadDevice();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Reads device configuration and checks for WFB YAML support
    /// </summary>
    public async Task ReadDevice()
    {
        var cts = new CancellationTokenSource(30000); // 30 seconds timeout

        try
        {
            if (DeviceConfig.Instance.DeviceType != DeviceType.None)
            {
                await CheckWfbYamlSupport(cts.Token);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error reading device configuration");
        }
        finally
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Checks if the device supports WFB YAML configuration
    /// </summary>
    private async Task CheckWfbYamlSupport(CancellationToken cancellationToken)
    {
        try
        {
            var cmdResult = await GetIsWfbYamlSupported(cancellationToken);
            
            IsWfbYamlEnabled = bool.TryParse(Utilities.RemoveLastChar(cmdResult?.Result), out var result) && result;

            // TODO: check if wfb.yaml exists when all parameters are supported
            // https://github.com/svpcom/wfb-ng/wiki/Drone-auto-provisioning
            //IsWfbYamlEnabled = false;

            Logger.Debug($"WFB YAML support status: {IsWfbYamlEnabled}");
        }
        catch (Exception ex)
        {
            Logger.Error("Error checking WFB YAML support: " + ex.Message);
            IsWfbYamlEnabled = false;
        }
    }

    /// <summary>
    /// Executes command to check if WFB YAML file exists on device
    /// </summary>
    private async Task<SshCommand?> GetIsWfbYamlSupported(CancellationToken cancellationToken)
    {
        var command = "test -f /etc/wfb.yaml && echo 'true' || echo 'false'";
        var cmdResult = await SshClientService.ExecuteCommandWithResponseAsync(
            DeviceConfig.Instance,
            command,
            cancellationToken);

        return cmdResult;
    }
    #endregion
}