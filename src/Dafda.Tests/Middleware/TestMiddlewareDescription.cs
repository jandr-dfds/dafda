using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestMiddlewareDescription
{
    [Fact]
    public void can_describe_middleware()
    {
        var middleware = new Middleware<int, string>();

        var description = MiddlewareDescription.Describe(middleware);

        Assert.Same(description.Instance, middleware);
        Assert.Same(description.InContextType, typeof(int));
        Assert.Same(description.OutContextType, typeof(string));
        Assert.NotNull(description.InvokeMethod);
    }

    [Fact]
    public async Task has_correct_method_info()
    {
        string recordedValue = null;
        var middleware = new Middleware<int, string>(ctx =>  ctx.ToString());
        var description = MiddlewareDescription.Describe(middleware);

        await ((Task)description.InvokeMethod.Invoke(middleware, new object[] { 1, Spy }))!;

        Assert.Equal("1", recordedValue);

        Task Spy(string s)
        {
            recordedValue = s;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void can_handle_missing_implementation()
    {
        var middleware = new MiddlewareWithoutImplementation();

        Assert.Throws<InvalidOperationException>(() => MiddlewareDescription.Describe(middleware));
    }

    [Fact]
    public void can_handle_null_middleware()
    {
        Assert.Throws<ArgumentNullException>(() => MiddlewareDescription.Describe(null));
    }

    private class Middleware<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        private readonly Func<TInContext, TOutContext> _next;

        public Middleware(Func<TInContext, TOutContext> next = null)
        {
            _next = next;
        }

        public Task Invoke(TInContext context, Func<TOutContext, Task> next)
        {
            return next(_next(context));
        }
    }

    private class MiddlewareWithoutImplementation : IMiddleware
    {
    }
}