using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class SetupCameraButtonsView : UserControl
{
    public SetupCameraButtonsView()
    {
        InitializeComponent();
        //if (!Design.IsDesignMode) DataContext = new SetupTabViewModel();
        DataContext = App.ServiceProvider.GetService<SetupTabViewModel>();
    }
}