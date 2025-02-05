using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class WfbTabView : UserControl
{
    public WfbTabView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
            // Resolve the DataContext from the DI container at runtime
            DataContext = App.ServiceProvider.GetService<WfbTabViewModel>();
    }
}