using Avalonia.Controls;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class LogViewer : UserControl
{
    public LogViewer()
    {
        InitializeComponent();

        if (!Design.IsDesignMode) DataContext = new LogViewerViewModel();
    }
}