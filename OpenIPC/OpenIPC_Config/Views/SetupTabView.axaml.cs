using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenIPC_Config.ViewModels;

namespace OpenIPC_Config.Views;

public partial class SetupTabView : UserControl
{
    public SetupTabView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = new SetupTabViewModel();
        }
        

    }
    
    
}