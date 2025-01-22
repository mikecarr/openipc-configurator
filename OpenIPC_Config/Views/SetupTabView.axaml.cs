using Avalonia.Controls;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class SetupTabView : UserControl
{
    public SetupTabView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = App.ServiceProvider.GetService<SetupTabViewModel>();
        }
    }
    
}