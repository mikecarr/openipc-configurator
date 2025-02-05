using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using OpenIPC_Config.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace OpenIPC_Config.Views;

public partial class PresetsTabView : UserControl
{
    public PresetsTabView()
    {
        InitializeComponent();

        if (!Design.IsDesignMode)
        {
            try
            {
                // Resolve the ViewModel from the DI container
                var viewModel = App.ServiceProvider.GetService<PresetsTabViewModel>();
                if (viewModel == null)
                {
                    throw new InvalidOperationException("Failed to resolve PresetsTabViewModel from the service provider.");
                }

                // Set the DataContext
                DataContext = viewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing PresetsTabView: {ex.Message}");
                // Optionally, provide a fallback or handle errors gracefully
            }
        }
        
    }

    
}