using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC.ViewModels;
using Prism.Events;

namespace OpenIPC.Views;

public partial class Tab1 : UserControl
{
    private readonly IEventAggregator _eventAggregator;
    
    public Tab1()
    {
        InitializeComponent();
        
        var eventAggregator = App.EventAggregator;
        
        if (!Design.IsDesignMode)
        {
            DataContext = new Tab1ViewModel();
        }
    }
}