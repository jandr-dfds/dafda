using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Middleware;

#nullable enable
public class TestPipeline
{
    [Fact]
    public async Task can_invoke_middleware_for_same_context_type()
    {
        var spy = MiddlewareFactory.CreateContextSpy();
        var pipeline = new Pipeline(
            spy.CreateMiddleware<SomeContext>(),
            spy.CreateMiddleware<SomeContext>()
        );

        var startingContext = new SomeContext();
        await pipeline.Invoke(startingContext);

        Assert.Equal(new object[]
        {
            startingContext,
            startingContext,
        }, spy.RecordedContexts);
    }

    [Fact]
    public async Task can_invoke_middleware_when_changing_context_type()
    {
        var spy = MiddlewareFactory.CreateContextSpy();
        var someOtherContext = new SomeOtherContext();
        var pipeline = new Pipeline(
            spy.CreateMiddleware<SomeContext, SomeOtherContext>(_ => someOtherContext),
            spy.CreateMiddleware<SomeOtherContext>()
        );

        var startingContext = new SomeContext();
        await pipeline.Invoke(startingContext);

        Assert.Equal(new object[]
        {
            startingContext,
            someOtherContext,
        }, spy.RecordedContexts);
    }

    [Fact]
    public async Task same_service_provider_throughout_pipeline()
    {
        var spy = new ForwardContextMiddleware<ContextStub>();
        var pipeline = new Pipeline(
            new ForwardContextMiddleware<RootMiddlewareContext, ContextStub>(c => new ContextStub(c)),
            new ForwardContextMiddleware<ContextStub, ContextStub>(c => new ContextStub(c)),
            new ForwardContextMiddleware<ContextStub, ContextStub>(c => new ContextStub(c)),
            spy
        );

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var startingContext = new RootMiddlewareContext(serviceProvider);
        await pipeline.Invoke(startingContext);

        Assert.Equal(serviceProvider, spy.OutContext?.ServiceProvider);
    }

    [Fact]
    public async Task can_handle_no_middleware()
    {
        var pipeline = new Pipeline();

        await pipeline.Invoke(new SomeContext());
    }

    private class SomeContext : DummyMiddlewareContext
    {
    }

    private class SomeOtherContext : DummyMiddlewareContext
    {
    }

    private class ForwardContextMiddleware<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
        where TInContext : IMiddlewareContext
        where TOutContext : IMiddlewareContext
    {
        private readonly Func<TInContext, TOutContext> _transform;

        public ForwardContextMiddleware(Func<TInContext, TOutContext> transform)
        {
            _transform = transform;
        }

        public Task Invoke(TInContext context, Func<TOutContext, Task> next)
        {
            var outContext = _transform(context);
            return next(outContext);
        }
    }

    private class ForwardContextMiddleware<TInContext> : IMiddleware<TInContext, ContextSpy>
        where TInContext : IMiddlewareContext
    {
        public Task Invoke(TInContext context, Func<ContextSpy, Task> next)
        {
            OutContext = new ContextSpy(context);
            return next(OutContext);
        }

        public ContextSpy? OutContext { get; private set; }
    }

    private class ContextStub : MiddlewareContext
    {
        public ContextStub(IMiddlewareContext parent) : base(parent)
        {
        }
    }

    private class ContextSpy : MiddlewareContext
    {
        public ContextSpy(IMiddlewareContext parent) : base(parent)
        {
        }
    }
}