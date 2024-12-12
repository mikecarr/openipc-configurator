using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class TelemetryTabView : UserControl
{
    public TelemetryTabView()
    {
        InitializeComponent();

        //if (!Design.IsDesignMode) DataContext = new TelemetryTabViewModel();
        DataContext = App.ServiceProvider.GetService<TelemetryTabViewModel>();
    }
}