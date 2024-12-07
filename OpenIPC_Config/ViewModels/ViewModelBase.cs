using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.Services;
using Prism.Events;

namespace OpenIPC_Config.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private readonly IEventAggregator _eventAggregator = App.EventAggregator;
    
    
    public async void UpdateUIMessage(string message)
    {
        _eventAggregator.GetEvent<AppMessageEvent>().Publish(new AppMessage
        {
            Message = message,
            UpdateLogView = false
        });
    }
}