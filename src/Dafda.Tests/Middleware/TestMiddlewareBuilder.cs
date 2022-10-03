using Dafda.Middleware;

namespace Dafda.Tests.Middleware;

public class TestMiddlewareBuilder
{
    [Fact]
    public void can_register_middleware()
    {
        var aa = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var ab = MiddlewareFactory.CreateDummy<IContextA, IContextB>();
        var ba = MiddlewareFactory.CreateDummy<IContextB, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>()
            .Register(aa)
            .Register(ab)
            .Register(ba)
            .Register(aa)
            .Build();

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
        var middleware1 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var middleware2 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();
        var middleware3 = MiddlewareFactory.CreateDummy<IContextA, IContextA>();

        var middlewares = new MiddlewareBuilder<IContextA>()
            .RegisterAll(new[]
            {
                middleware1,
                middleware2,
                middleware3
            })
            .Build();

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