using System;
using System.Collections.Generic;
using Dafda.Producing;

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

        /// <inheritdoc />
        public void Set<T>(T instance)
        {
            _parent.Set(instance);
        }

        /// <inheritdoc />
        public T Get<T>()
        {
            return _parent.Get<T>();
        }
    }

    internal class RootMiddlewareContext : IMiddlewareContext
    {
        private readonly IDictionary<Type, object> _contextBag = new Dictionary<Type, object>();

        public RootMiddlewareContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public void Set<T>(T instance)
        {
            _contextBag[typeof(T)] = instance;
        }

        public T Get<T>()
        {
            if (_contextBag.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }
            return default(T);
        }
    }

    internal class RootProducerMiddlewareContext : RootMiddlewareContext
    {
        public RootProducerMiddlewareContext(IServiceProvider serviceProvider, KafkaProducer kafkaProducer)
            : base(serviceProvider)
        {
            Set(kafkaProducer);
        }
    }
}