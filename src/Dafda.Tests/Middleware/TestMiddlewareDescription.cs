using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Middleware;

public class TestMiddlewareDescription
{
    [Fact]
    public void can_describe_middleware()
    {
        var middleware = MiddlewareFactory.CreateStub<int, string>();

        var description = MiddlewareDescription.Describe(middleware);

        Assert.Same(description.Instance, middleware);
        Assert.Same(description.InContextType, typeof(MiddlewareFactory.ValueContext<int>));
        Assert.Same(description.OutContextType, typeof(MiddlewareFactory.ValueContext<string>));
        Assert.NotNull(description.InvokeMethod);
    }

    [Fact]
    public async Task has_correct_method_info()
    {
        string recordedValue = null;
        var middleware = MiddlewareFactory.CreateStub<int, string>(ctx =>  ctx.ToString());
        var description = MiddlewareDescription.Describe(middleware);

        await ((Task)description.InvokeMethod.Invoke(middleware, new object[] { new MiddlewareFactory.ValueContext<int>(1), Spy }))!;

        Assert.Equal("1", recordedValue);

        Task Spy(MiddlewareFactory.ValueContext<string> context)
        {
            recordedValue = context.Value;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void can_handle_missing_implementation()
    {
        var middleware = MiddlewareFactory.CreateInvalid();

        Assert.Throws<InvalidOperationException>(() => MiddlewareDescription.Describe(middleware));
    }

    [Fact]
    public void can_handle_null_middleware()
    {
        Assert.Throws<ArgumentNullException>(() => MiddlewareDescription.Describe(null));
    }
}