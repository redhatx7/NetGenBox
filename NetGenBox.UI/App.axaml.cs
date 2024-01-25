using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace NetGenBox.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static IConfiguration Configuration =>
        new ConfigurationBuilder().AddJsonFile("config.json")
            .Build();
    
    
    public override void OnFrameworkInitializationCompleted()
    {
        Console.WriteLine("ISE PATH=" + Configuration["isePath"]);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            var mainWindow = new MainWindow()
            {
                Configuration = Configuration,
            };
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}