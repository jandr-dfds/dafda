using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestMiddlewareBuilder
{
    [Fact]
    public void can_register_middleware()
    {
        var services = new ServiceCollection();
        var aa = new Middleware<IContextA, IContextA>();
        var ab = new Middleware<IContextA, IContextB>();
        var ba = new Middleware<IContextB, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>(services)
            .Register(_ => aa)
            .Register(_ => ab)
            .Register(_ => ba)
            .Register(_ => aa)
            .Build(services.BuildServiceProvider());

        Assert.Equal(new IMiddleware[]
        {
            aa,
            ab,
            ba,
            aa,
        }, middlewares);
    }

    [Fact]
    public void can_register_all_middleware()
    {
        var services = new ServiceCollection();
        var middleware1 = new Middleware<IContextA, IContextA>();
        var middleware2 = new Middleware<IContextA, IContextA>();
        var middleware3 = new Middleware<IContextA, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>(services)
            .RegisterAll(new Func<IServiceProvider, IMiddleware<IContextA, IContextA>>[]
            {
                _ => middleware1,
                _ => middleware2,
                _ => middleware3
            })
            .Build(services.BuildServiceProvider());

        Assert.Equal(new IMiddleware[]
        {
            middleware1,
            middleware2,
            middleware3,
        }, middlewares);
    }

    private interface IContextA
    {
    }

    private interface IContextB
    {
    }

    private class Middleware<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        public Task Invoke(TInContext context, Func<TOutContext, Task> next) => Task.CompletedTask;
    }
}