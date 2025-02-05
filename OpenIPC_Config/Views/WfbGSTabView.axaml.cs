using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

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