using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class WfbGSTabView : UserControl
{
    public WfbGSTabView()
    {
        InitializeComponent();

        //if (!Design.IsDesignMode) DataContext = new WfbGSTabViewModel();
        DataContext = App.ServiceProvider.GetService<WfbGSTabViewModel>();
    }
}