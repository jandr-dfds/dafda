using System;
using System.Threading.Tasks;
using Dafda.Middleware;

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
}