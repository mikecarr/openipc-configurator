using Avalonia.Controls;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class MainView : UserControl
{
    public static TabControl TabControlInstance { get; set; }
    
    public MainView()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new MainViewModel();
        }
        
        TabControlInstance = this.FindControl<TabControl>("MainTabControl");

        var eventAggregator = App.EventAggregator;
        eventAggregator.GetEvent<DeviceTypeChangeEvent>().Publish(DeviceType.None);
        
    }
}