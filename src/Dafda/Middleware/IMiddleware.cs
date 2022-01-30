using System.Threading.Tasks;

namespace Dafda.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IMiddleware<TContext>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        Task Invoke(TContext context, MiddlewareDelegate next);
    }
}