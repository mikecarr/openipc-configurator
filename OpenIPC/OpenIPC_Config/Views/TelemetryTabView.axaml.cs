using Avalonia;
using Avalonia.Controls;
using OpenIPC_Config.ViewModels;



namespace OpenIPC_Config.Views;

public partial class TelemetryTabView : UserControl
{
    public TelemetryTabView()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new TelemetryTabViewModel();
        }
    }
}