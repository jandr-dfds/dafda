using Dafda.Middleware;

namespace Dafda.Tests.Builders
{
    public class MiddlewareEngineBuilder<TContext>
    {
        public MiddlewareEngine<TContext> Build()
        {
            return new MiddlewareEngine<TContext>();
        }
    }
}