using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenIPC_Config.Services;
using OpenIPC_Config.ViewModels;
using Serilog;

namespace OpenIPC_Config.Tests;

public class DependencyInjectionTests
{
    [Test]
    public void ViewModel_CanBeResolvedFromDI()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IEventSubscriptionService, EventSubscriptionService>();
        services.AddSingleton<ISshClientService, SshClientService>();
        services.AddSingleton<IYamlConfigService, YamlConfigService>();
        services.AddSingleton<ILogger>(sp => Mock.Of<ILogger>());

        services.AddTransient<CameraSettingsTabViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var viewModel = serviceProvider.GetService<CameraSettingsTabViewModel>();

        // Assert
        Assert.IsNotNull(viewModel);
    }
}