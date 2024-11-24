using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class PresetsTabView : UserControl
{
    public PresetsTabView()
    {
        InitializeComponent();
        
        //if (!Design.IsDesignMode) DataContext = new PresetsTabViewModel();
        
        var viewModel = new PresetsTabViewModel();
        DataContext = viewModel;

        Console.WriteLine($"DataContext is: {DataContext?.GetType().Name ?? "null"}");
        
    }
}