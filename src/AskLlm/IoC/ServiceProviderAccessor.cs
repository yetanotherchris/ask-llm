using System;
using Microsoft.Extensions.DependencyInjection;

namespace AskLlm.IoC;

public static class ServiceProviderAccessor
{
    private static IServiceProvider? _serviceProvider;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public static IServiceProvider Provider => _serviceProvider ?? throw new InvalidOperationException("The service provider has not been configured.");

    public static T GetRequiredService<T>() where T : notnull
    {
        return Provider.GetRequiredService<T>();
    }
}
