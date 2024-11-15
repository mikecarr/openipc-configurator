using OpenIPC_Config.Events;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public class StatusBarViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;
    private string _hostNameText;

    private string _messageText;

    private string _statusText;

    public StatusBarViewModel()
    {
        _eventAggregator = App.EventAggregator;
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(UpdateStatus);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string MessagesText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public string HostNameText
    {
        get => _hostNameText;
        set => SetProperty(ref _hostNameText, value);
    }

    private void UpdateStatus(AppMessage appMessage)
    {
        Log.Debug(appMessage.ToString());


        if (!string.IsNullOrEmpty(appMessage.Status)) StatusText = appMessage.Status;
        if (!string.IsNullOrEmpty(appMessage.Message)) MessagesText = appMessage.Message;

        if (!string.IsNullOrEmpty(appMessage.DeviceConfig.Hostname)) HostNameText = appMessage.DeviceConfig.Hostname;
    }
}