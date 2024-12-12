using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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