using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class VRXTabView : UserControl
{
    public VRXTabView()
    {
        InitializeComponent();

        //if (!Design.IsDesignMode) DataContext = new VRXTabViewModel();
        DataContext = App.ServiceProvider.GetService<VRXTabViewModel>();
    }
}