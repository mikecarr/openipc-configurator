using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC.ViewModels;

namespace OpenIPC.Views;

public partial class LogViewer : UserControl
{

    public LogViewer()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode)
        {
            DataContext = new LogViewerViewModel();
        }
    }
}