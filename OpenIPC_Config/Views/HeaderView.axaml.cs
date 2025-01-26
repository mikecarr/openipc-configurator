using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class HeaderView : UserControl
{
    public HeaderView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode) DataContext = App.ServiceProvider.GetService<MainViewModel>();
    }
}