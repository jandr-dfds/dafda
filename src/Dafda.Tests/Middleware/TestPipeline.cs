using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Tests.TestDoubles;
using Xunit;

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

    private class SomeContext
    {
    }

    private class SomeOtherContext
    {
    }
}