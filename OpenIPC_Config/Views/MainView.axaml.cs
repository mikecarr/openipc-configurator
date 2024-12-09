using System;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.ViewModels;
using Prism.Events;
using Serilog;

namespace OpenIPC_Config.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            // Resolve MainViewModel from the DI container
            DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>();

            // Subscribe to TabSelectionChangeEvent
            var eventAggregator = App.ServiceProvider.GetRequiredService<IEventAggregator>();
            eventAggregator.GetEvent<TabSelectionChangeEvent>().Subscribe(OnTabSelectionChanged);
        }
        else
        {
            // Provide a default DataContext for design-time
            DataContext = App.ServiceProvider.GetRequiredService<MainViewModel>(); 
        }
    }

    private void OnTabSelectionChanged(string selectedTab)
    {
        // Find the TabControl
        var tabControl = this.FindControl<TabControl>("MainTabControl");

        // Find the target TabItem by its Header
        var targetTab = tabControl.Items
            .OfType<TabItem>()
            .FirstOrDefault(tab => tab.Header?.ToString() == selectedTab);

        if (targetTab != null)
        {
            targetTab.IsSelected = true;
        }
    }
}