using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class NetworkIPScannerView : UserControl
{
    public NetworkIPScannerView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode) DataContext = App.ServiceProvider.GetService<SetupTabViewModel>();
    }
}