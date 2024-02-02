using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace NetGenBox.UI;

public partial class App : Application
{
    
    private IServiceProvider _serviceProvider;
    
    public override void Initialize()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        ServiceLocator.SetProvider(_serviceProvider);
        ServiceLocator.SetConfiguration(Configuration);
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<MainWindow>();
        serviceCollection.AddSingleton<ProjectConfiguration>();
    }

    public static IConfiguration Configuration =>
        new ConfigurationBuilder().AddJsonFile("config.json")
            .Build();
    
    
    public override void OnFrameworkInitializationCompleted()
    {
        //SukiTheme.GetInstance().ChangeBaseTheme(ThemeVariant.Light);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Configuration = Configuration;
            desktop.MainWindow = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
}