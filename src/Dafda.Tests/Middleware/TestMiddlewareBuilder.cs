using System;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Middleware;

public class TestMiddlewareBuilder
{
    [Fact]
    public void can_register_middleware()
    {
        var services = new ServiceCollection();
        var aa = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var ab = MiddlewareFactory.CreateDummy<IContextA, IContextB>();
        var ba = MiddlewareFactory.CreateDummy<IContextB, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>(services)
            .Register(aa)
            .Register(ab)
            .Register(ba)
            .Register(aa)
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
        var middleware1 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var middleware2 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var middleware3 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>(services)
            .RegisterAll(new IMiddleware<IContextA, IContextA>[]
            {
                middleware1,
                middleware2,
                middleware3
            })
            .Build(services.BuildServiceProvider());

        Assert.Equal(new IMiddleware[]
        {
            middleware1,
            middleware2,
            middleware3,
        }, middlewares);
    }

    private interface IContextA : IMiddlewareContext
    {
    }

    private interface IContextB : IMiddlewareContext
    {
    }
}