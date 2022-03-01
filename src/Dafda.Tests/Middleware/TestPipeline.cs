using System;
using System.Collections.Generic;
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
        var spy = new ContextRecorder();
        var pipeline = new Pipeline(
            FakeMiddleware<SomeContext>.WithPreAction(spy.Record),
            FakeMiddleware<SomeContext>.WithPreAction(spy.Record)
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
        var spy = new ContextRecorder();
        var someOtherContext = new SomeOtherContext();
        var pipeline = new Pipeline(
            new FakeMiddleware<SomeContext, SomeOtherContext>(_ => someOtherContext, spy.Record),
            FakeMiddleware<SomeOtherContext>.WithPreAction(spy.Record)
        );

        var startingContext = new SomeContext();
        await pipeline.Invoke(startingContext);

        Assert.Equal(new object[]
        {
            startingContext,
            someOtherContext,
        }, spy.RecordedContexts);
    }

    private class ContextRecorder
    {
        private readonly IList<object> _recordedContexts = new List<object>();

        public void Record<T>(T context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _recordedContexts.Add(context);
        }

        public IEnumerable<object> RecordedContexts => _recordedContexts;
    }

    private class SomeContext
    {
    }

    private class SomeOtherContext
    {
    }
}