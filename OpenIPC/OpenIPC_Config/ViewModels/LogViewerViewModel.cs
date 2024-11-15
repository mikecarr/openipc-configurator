using System;
using System.Collections.ObjectModel;
using OpenIPC_Config.Events;
using Prism.Events;

namespace OpenIPC_Config.ViewModels;

public class LogViewerViewModel : ViewModelBase
{
    private readonly IEventAggregator _eventAggregator;

    private string _messageText;

    public LogViewerViewModel()
    {
        _eventAggregator = App.EventAggregator;
        LogMessages = new ObservableCollection<string>();
        _eventAggregator.GetEvent<AppMessageEvent>().Subscribe(AppMessageReceived);
        _eventAggregator.GetEvent<LogMessageEvent>().Subscribe(LogMessageReceived);
    }

    public ObservableCollection<string> LogMessages { get; set; }

    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    private void LogMessageReceived(string message)
    {
        var formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

        //Log.Debug("Serilog: " + message);
        if (!message.Contains("Ping"))
            LogMessages.Insert(0, formattedMessage);
    }

    private void AppMessageReceived(AppMessage message)
    {
        var formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

        if (message.UpdateLogView) LogMessages.Insert(0, formattedMessage);
    }
}