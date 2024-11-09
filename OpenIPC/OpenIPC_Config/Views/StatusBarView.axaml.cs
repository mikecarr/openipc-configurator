using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC_Config.ViewModels;


namespace OpenIPC_Config.Views;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            DataContext = new StatusBarViewModel();
        }
        
    }
}