#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dafda.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class MiddlewareEngine<TContext>
    {
        private readonly LinkedList<IMiddleware<TContext>> _middlewares = new LinkedList<IMiddleware<TContext>>();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        public void AddMiddleware(IEnumerable<IMiddleware<TContext>> list)
        {
            foreach (var middleware in list)
            {
                _middlewares.AddLast(middleware);
            }
        }

        private static async Task InnerExecute(TContext context, LinkedListNode<IMiddleware<TContext>>? current)
        {
            if (current is null)
            {
                return;
            }
            
            var next = current.Next;
            await current.Value.Invoke(context, _ => InnerExecute(context, next));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="innerMostAction"></param>
        /// <returns></returns>
        public Task Execute(TContext context, Func<TContext, Task> innerMostAction)
        {
            var pipeline = new LinkedList<IMiddleware<TContext>>();
            foreach (var middleware in _middlewares)
            {
                pipeline.AddLast(middleware);
            }

            pipeline.AddLast(new EndOfLine(innerMostAction));
            
            return InnerExecute(context, pipeline.First);
        }
        
        private class EndOfLine : IMiddleware<TContext>
        {
            private readonly Func<TContext, Task> _action;

            public EndOfLine(Func<TContext, Task> action) => _action = action;
            public Task Invoke(TContext context, Func<TContext, Task> next)
            {
                return _action(context);
            }
        }
    }
}