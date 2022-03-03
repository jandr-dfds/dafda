using System;
using System.Threading.Tasks;
using Dafda.Middleware;

namespace Dafda.Tests.TestDoubles;

internal static class MiddlewareFactory
{
    public static MiddlewareSpy<TInContext, TOutContext> DecorateWithSpy<TInContext, TOutContext>(IMiddleware<TInContext, TOutContext> middleware)
    {
        return new MiddlewareSpy<TInContext, TOutContext>(middleware);
    }

    public class MiddlewareSpy<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        private readonly IMiddleware<TInContext, TOutContext> _inner;

        public MiddlewareSpy(IMiddleware<TInContext, TOutContext> inner)
        {
            _inner = inner;
        }

        public Task Invoke(TInContext context)
        {
            return ((IMiddleware<TInContext, TOutContext>)this).Invoke(context, outContext =>
            {
                OutContext = outContext;
                return Task.CompletedTask;
            });
        }

        Task IMiddleware<TInContext, TOutContext>.Invoke(TInContext context, Func<TOutContext, Task> next)
        {
            return _inner.Invoke(context, next);
        }

        public TOutContext OutContext { get; private set; }
    }

    public static IMiddleware<TInContext, TOutContext> CreateDummy<TInContext, TOutContext>()
    {
        return new MiddlewareDummy<TInContext, TOutContext>();
    }

    private class MiddlewareDummy<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        public Task Invoke(TInContext context, Func<TOutContext, Task> next) => Task.CompletedTask;
    }
}