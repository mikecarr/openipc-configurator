using System;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected readonly ISshClientService SshClientService;
    protected readonly ILogger Logger;
    protected readonly IEventAggregator EventAggregator;
    
    protected ViewModelBase(
        ILogger logger,
        ISshClientService sshClientService,
        IEventAggregator eventAggregator)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SshClientService = sshClientService ?? throw new ArgumentNullException(nameof(sshClientService));
        EventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
    }
    
    public async void UpdateUIMessage(string message)
    {
        EventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage
        {
            Message = message,
            UpdateLogView = false
        });
    }
}