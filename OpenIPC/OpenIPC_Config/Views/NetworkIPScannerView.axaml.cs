using Avalonia.Controls;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class NetworkIPScannerView : UserControl
{
    public NetworkIPScannerView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode) DataContext = new SetupTabViewModel();
    }
}