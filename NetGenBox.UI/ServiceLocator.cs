using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetGenBox.UI;

public class ServiceLocator
{
    private static IConfiguration _configuration;
    
    private static IServiceProvider _provider;

    public static void SetProvider(IServiceProvider provider) => _provider = provider;

    public static void SetConfiguration(IConfiguration configuration) => _configuration = configuration;

    public static T? GetService<T>() where T : class => _provider.GetService<T>();

    public static string? GetConfiguration(string key) => _configuration[key];
}