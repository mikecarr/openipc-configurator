using System;
using System.Collections.ObjectModel;
using OpenIPC_Config.Events;
using OpenIPC_Config.Services;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.ViewModels;

public class LogViewerViewModel : ViewModelBase
{
    private readonly IEventSubscriptionService _eventSubscriptionService;

    private string _messageText;
    
    private string _lastMessage = string.Empty;
    private int _duplicateCount = 0;
    private DateTime _lastFlushTime = DateTime.Now;

    public LogViewerViewModel(ILogger logger,
        ISshClientService sshClientService,
        IEventSubscriptionService eventSubscriptionService)
        : base(logger, sshClientService, eventSubscriptionService)
    {
        
        
        LogMessages = new ObservableCollection<string>();
        
        _eventSubscriptionService = eventSubscriptionService ??
                                    throw new ArgumentNullException(nameof(eventSubscriptionService));
        
        _eventSubscriptionService.Subscribe<AppMessageEvent, AppMessage>(
            AppMessageReceived);
        _eventSubscriptionService.Subscribe<LogMessageEvent, string>(
            LogMessageReceived);
        
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

        // Check if the current message is the same as the previous one
        if (message == _lastMessage)
        {
            _duplicateCount++;

            // Periodically flush duplicate count to the log (e.g., every 5 seconds)
            if ((DateTime.Now - _lastFlushTime).TotalSeconds >= 5 && _duplicateCount > 0)
            {
                FlushDuplicateMessage();
            }
        }
        else
        {
            // Flush any existing duplicate message summary before adding a new message
            if (_duplicateCount > 0)
            {
                FlushDuplicateMessage();
            }

            // Reset the duplicate counter and update the last message
            _duplicateCount = 0;
            _lastMessage = message;

            // Add the new message to the log
            LogMessages.Insert(0, formattedMessage);

        }
    }
    
    // Helper method to flush the duplicate message summary
    private void FlushDuplicateMessage()
    {
        if (_duplicateCount > 0)
        {
            var duplicateMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - [Last message repeated {_duplicateCount} times]";
            LogMessages.Insert(0, duplicateMessage);
            _duplicateCount = 0;
            _lastFlushTime = DateTime.Now;
        }
    }

    private void AppMessageReceived(AppMessage message)
    {
        var formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

        if (message.UpdateLogView)
        {
            LogMessages.Insert(0, formattedMessage);
        }
    }
}