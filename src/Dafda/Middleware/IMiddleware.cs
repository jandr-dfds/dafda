using System;
using System.Threading.Tasks;

namespace Dafda.Middleware
{
    /// <summary>
    /// Marker interface. Do NOT implement directly, instead use <see cref="IMiddleware{TInContext,TOutContext}"/>
    /// </summary>
    public interface IMiddleware
    {
    }

    /// <summary>
    /// Base interface for Middleware context.
    /// </summary>
    public interface IMiddlewareContext
    {
        /// <summary>
        /// <see cref="IServiceProvider"/> for the current executing middleware context. 
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
    
    /// <summary>
    /// This interface can be used to implement middleware that can be used in both incoming
    /// as well as outgoing pipelines. 
    /// </summary>
    /// <typeparam name="TInContext">The input context.</typeparam>
    /// <typeparam name="TOutContext">The output context.</typeparam>
    public interface IMiddleware<in TInContext, out TOutContext> : IMiddleware
        where TInContext : IMiddlewareContext
        where TOutContext : IMiddlewareContext
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
}