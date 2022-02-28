using System;
using System.Threading.Tasks;

namespace Dafda.Middleware
{
    /// <summary>
    /// This interface can be used to implement middleware that can be used in both incoming
    /// as well as outgoing pipelines. 
    /// </summary>
    /// <typeparam name="TInContext">The input context.</typeparam>
    /// <typeparam name="TOutContext">The output context.</typeparam>
    public interface IMiddleware<in TInContext, out TOutContext>
    {
        /// <summary>
        /// Executes the desired middleware behavior.
        /// </summary>
        /// <remarks>
        /// It is up to the implementation to insure that the <paramref name="next"/> is called.
        /// Failing to do so will shortcut the pipeline, which can also be used to ones advantage.
        /// </remarks>
        /// <param name="context">The executing context.</param>
        /// <param name="next">The next middleware in the pipeline.</param>
        Task Invoke(TInContext context, Func<TOutContext, Task> next);
    }

    /// <summary>
    /// This interface can be used to implement middleware that can be used in both incoming
    /// as well as outgoing pipelines, where the input and output contexts are the same. 
    /// </summary>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <seealso cref="IMiddleware{TInContext,TOutContext}"/>
    public interface IMiddleware<TContext> : IMiddleware<TContext, TContext>
    {
    }
}