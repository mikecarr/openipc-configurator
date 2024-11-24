using CommunityToolkit.Mvvm.ComponentModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public partial class StatusBarViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    
    [ObservableProperty] private string _hostNameText;

    [ObservableProperty]  private string _messageText;

    [ObservableProperty] private string _statusText;

    [ObservableProperty] private string _appVersionText;
    
    public StatusBarViewModel()
    {
        _eventAggregator = App.EventAggregator;
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(UpdateStatus);

        _appVersionText = VersionHelper.GetAppVersion();
    }

    

    private void UpdateStatus(AppMessage appMessage)
    {
        Log.Verbose(appMessage.ToString());


        if (!string.IsNullOrEmpty(appMessage.Status)) StatusText = appMessage.Status;
        if (!string.IsNullOrEmpty(appMessage.Message)) MessageText = appMessage.Message;

        if (!string.IsNullOrEmpty(appMessage.DeviceConfig.Hostname)) HostNameText = appMessage.DeviceConfig.Hostname;
    }
}