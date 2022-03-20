using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration;

internal static class ServiceCollectionExtensions
{
    public static T GetOrAddSingleton<T>(this IServiceCollection services, Func<T> factory) where T : class
    {
        var instance = services
            .Where(x => x.ServiceType == typeof(T))
            .Where(x => x.Lifetime == ServiceLifetime.Singleton)
            .Where(x => x.ImplementationInstance != null)
            .Where(x => x.ImplementationInstance.GetType() == typeof(T))
            .Select(x => x.ImplementationInstance)
            .Cast<T>()
            .SingleOrDefault();

        if (instance == null)
        {
            instance = factory();
            services.AddSingleton(instance);
        }

        return instance;
    }
}