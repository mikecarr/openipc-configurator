using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();

        //if (!Design.IsDesignMode) DataContext = new StatusBarViewModel();
        DataContext = App.ServiceProvider.GetService<StatusBarViewModel>();
    }
}