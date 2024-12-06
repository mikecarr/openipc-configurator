using System;
using System.Linq;
using Avalonia.Controls;
using OpenIPC_Config.Events;
using OpenIPC_Config.Models;
using OpenIPC_Config.ViewModels;
using Serilog;

namespace OpenIPC_Config.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        TabControlInstance = this.FindControl<TabControl>("MainTabControl");
        
        if (!Design.IsDesignMode) DataContext = new MainViewModel();
        
    }

    public static TabControl TabControlInstance { get; set; }
    
    
}