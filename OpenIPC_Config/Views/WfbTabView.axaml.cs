using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class WfbTabView : UserControl
{
    // Parameterless constructor for Avalonia
    public WfbTabView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            // Resolve the DataContext from the DI container at runtime
            //DataContext = App.ServiceProvider.GetRequiredService<WfbTabViewModel>();
            DataContext = App.ServiceProvider.GetService<WfbTabViewModel>();
        }
    }
    
    // public WfbTabView(WfbTabViewModel viewModel)
    // {
    //     InitializeComponent();
    //
    //     //if (!Design.IsDesignMode) DataContext = viewModel;
    //     
    //     // Resolve WfbTabViewModel using the IoC container
    //     DataContext = App.ServiceProvider.GetService<WfbTabViewModel>();
    // }
}