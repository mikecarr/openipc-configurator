using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class NetworkIPScannerView : UserControl
{
    public NetworkIPScannerView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = App.ServiceProvider.GetService<SetupTabViewModel>();
        }
    }
}