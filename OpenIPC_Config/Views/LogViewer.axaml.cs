using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class LogViewer : UserControl
{
    public LogViewer()
    {
        InitializeComponent();

        //if (!Design.IsDesignMode) DataContext = new LogViewerViewModel();
        DataContext = App.ServiceProvider.GetService<LogViewerViewModel>();
    }
}