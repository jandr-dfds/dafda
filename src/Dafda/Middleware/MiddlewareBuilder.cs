using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Middleware;

internal class MiddlewareBuilder<T> 
    where T : IMiddlewareContext
{
    private readonly IServiceCollection _services;
    private readonly List<Func<IServiceProvider, IMiddleware>> _registrations;

    public MiddlewareBuilder(IServiceCollection services)
        : this(services, new List<Func<IServiceProvider, IMiddleware>>())
    {
    }

    private MiddlewareBuilder(IServiceCollection services, List<Func<IServiceProvider, IMiddleware>> registrations)
    {
        _registrations = registrations;
        _services = services;
    }

    public MiddlewareBuilder<TOutContext> Register<TOutContext>(Func<IServiceProvider, IMiddleware<T, TOutContext>> factory) 
        where TOutContext : IMiddlewareContext
    {
        _services.AddTransient((Func<IServiceProvider, IMiddleware>)factory);
        _registrations.Add(factory);

        return new MiddlewareBuilder<TOutContext>(_services, _registrations);
    }

    public MiddlewareBuilder<T> RegisterAll(IEnumerable<Func<IServiceProvider, IMiddleware<T, T>>> factories)
    {
        foreach (var factory in factories)
        {
            Register(factory);
        }

        return this;
    }

    public IMiddleware[] Build(IServiceProvider serviceProvider)
    {
        return _registrations
            .Select(factory => factory(serviceProvider))
            .ToArray();
    }
}