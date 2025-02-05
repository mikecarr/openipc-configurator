using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

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