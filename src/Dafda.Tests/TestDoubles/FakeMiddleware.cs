using System;
using System.Threading.Tasks;
using Dafda.Middleware;

#nullable enable
namespace Dafda.Tests.TestDoubles
{
    public class FakeMiddleware<TContext> : IMiddleware<TContext>
    {
        private readonly Action<TContext> _pre;
        private readonly Action<TContext> _post;

        private FakeMiddleware(Action<TContext>? pre = null, Action<TContext>? post = null)
        {
            _pre = pre ?? ((_) => {});
            _post = post ?? ((_) => {});
        }

        public async Task Invoke(TContext context, Func<TContext, Task> next)
        {
            _pre(context);
            await next(context);
            _post(context);
        }
            
        public static FakeMiddleware<TContext> WithPreAction(Action<TContext> action) 
            => new FakeMiddleware<TContext>(pre: action);

        public static FakeMiddleware<TContext> WithPostAction(Action<TContext> action) 
            => new FakeMiddleware<TContext>(post: action);

        public static FakeMiddleware<TContext> WithPreAndPostActions(Action<TContext> preAction, Action<TContext> postAction) 
            => new FakeMiddleware<TContext>(pre: preAction, post: postAction);
    }

    public class FakeMiddleware<TInContext, TOutContext> : IMiddleware<TInContext, TOutContext>
    {
        private readonly Func<TInContext, TOutContext> _transformContext;
        private readonly Action<TInContext> _pre;
        private readonly Action<TInContext> _post;

        public FakeMiddleware(Func<TInContext, TOutContext> transformContext, Action<TInContext>? pre = null, Action<TInContext>? post = null)
        {
            _transformContext = transformContext;
            _pre = pre ?? (_ => { });
            _post = post ?? (_ => { });
        }

        public async Task Invoke(TInContext context, Func<TOutContext, Task> next)
        {
            _pre(context);
            await next(_transformContext(context));
            _post(context);
        }
    }
}