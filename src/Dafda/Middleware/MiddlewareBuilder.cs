using System.Collections.Generic;

namespace Dafda.Middleware;

internal class MiddlewareBuilder<T> 
    where T : IMiddlewareContext
{
    private readonly List<IMiddleware> _registrations;

    public MiddlewareBuilder()
        : this(new List<IMiddleware>())
    {
    }

    private MiddlewareBuilder(List<IMiddleware> registrations)
    {
        _registrations = registrations;
    }

    public MiddlewareBuilder<TOutContext> Register<TOutContext>(IMiddleware<T, TOutContext> middleware) 
        where TOutContext : IMiddlewareContext
    {
        _registrations.Add(middleware);

        return new MiddlewareBuilder<TOutContext>(_registrations);
    }

    public MiddlewareBuilder<T> RegisterAll(IEnumerable<IMiddleware<T, T>> factories)
    {
        _registrations.AddRange(factories);

        return this;
    }

    public IMiddleware[] Build()
    {
        return _registrations
            .ToArray();
    }
}