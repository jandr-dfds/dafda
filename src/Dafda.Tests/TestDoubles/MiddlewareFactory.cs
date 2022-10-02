using System;
using System.Collections.Generic;
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

    public static IMiddleware<TInContext, TOutContext> CreateStub<TInContext, TOutContext>(Func<TInContext, TOutContext> next = null)
    {
        return new MiddlewareStub<TInContext, TOutContext>(next);
    }

    private class MiddlewareStub<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        private readonly Func<TInContext, TOutContext> _next;

        public MiddlewareStub(Func<TInContext, TOutContext> next = null)
        {
            _next = next;
        }

        public Task Invoke(TInContext context, Func<TOutContext, Task> next)
        {
            return next(_next(context));
        }
    }

    public static IMiddleware CreateInvalid()
    {
        return new MiddlewareWithoutImplementation();
    }

    /// <summary>
    /// Middleware without implementation. <see cref="IMiddleware"/> is only a marker interface.
    /// </summary>
    private class MiddlewareWithoutImplementation : IMiddleware
    {
    }

    public static MiddlewareContextSpy CreateContextSpy()
    {
        return new MiddlewareContextSpy();
    }

    public class MiddlewareContextSpy
    {
        private readonly IList<object> _recordedContexts = new List<object>();

        public IMiddleware<TContext, TContext> CreateMiddleware<TContext>()
        {
            return CreateMiddleware<TContext, TContext>(context => context);
        }

        public IMiddleware<TInContext, TOutContext> CreateMiddleware<TInContext, TOutContext>(Func<TInContext, TOutContext> transformContext)
        {
            return new Spy<TInContext, TOutContext>(_recordedContexts, transformContext);
        }

        public IEnumerable<object> RecordedContexts => _recordedContexts;

        private class Spy<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
        {
            private readonly IList<object> _recordedContexts;
            private readonly Func<TInContext, TOutContext> _transformContext;

            public Spy(IList<object> recordedContexts, Func<TInContext, TOutContext> transformContext)
            {
                _recordedContexts = recordedContexts;
                _transformContext = transformContext;
            }

            public Task Invoke(TInContext context, Func<TOutContext, Task> next)
            {
                _recordedContexts.Add(context);
                return next(_transformContext(context));
            }
        }
    }
}