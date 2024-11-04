using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenIPC.ViewModels;


namespace OpenIPC.Views;

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