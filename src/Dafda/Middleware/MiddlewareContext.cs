using System;

namespace Dafda.Middleware
{
    /// <summary>
    /// Base class for Middleware context.
    /// </summary>
    public abstract class MiddlewareContext : IMiddlewareContext
    {
        private readonly IMiddlewareContext _parent;

        /// <summary/>
        protected MiddlewareContext(IMiddlewareContext parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _parent.ServiceProvider;
    }

    internal sealed class RootMiddlewareContext : IMiddlewareContext
    {
        public RootMiddlewareContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}