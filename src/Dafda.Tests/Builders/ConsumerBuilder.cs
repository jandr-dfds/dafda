using System;
using Dafda.Consuming;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.Builders
{
    internal class ConsumerBuilder
    {
        private ConsumerScope _consumerScope;

        private bool _enableAutoCommit;
        private IServiceScopeFactory _serviceScopeFactory;
        private MiddlewareBuilder<IncomingRawMessageContext> _middlewareBuilder;
        private ILogger<Consumer> _logger;

        public ConsumerBuilder()
        {
            _consumerScope = new ConsumerScopeStub(new MessageResultBuilder().Build());
            _serviceScopeFactory = new FakeServiceScopeFactory(type => throw new InvalidOperationException($"{type.Name} type registered"));
            _middlewareBuilder = new MiddlewareBuilder<IncomingRawMessageContext>(new ServiceCollection());
            _logger = NullLogger<Consumer>.Instance;
        }

        public ConsumerBuilder WithConsumerScope(ConsumerScope consumerScope)
        {
            _consumerScope = consumerScope;
            return this;
        }

        public ConsumerBuilder WithEnableAutoCommit(bool enableAutoCommit)
        {
            _enableAutoCommit = enableAutoCommit;
            return this;
        }

        public ConsumerBuilder WithServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            return this;
        }

        public ConsumerBuilder WithMiddleware(MiddlewareBuilder<IncomingRawMessageContext> middlewareBuilder)
        {
            _middlewareBuilder = middlewareBuilder;
            return this;
        }

        public ConsumerBuilder With(ILogger<Consumer> logger)
        {
            _logger = logger;
            return this;
        }

        public Consumer Build() =>
            new Consumer(_logger, () => _consumerScope, _serviceScopeFactory, _middlewareBuilder, _enableAutoCommit);

        private class FakeServiceScopeFactory : IServiceScopeFactory, IServiceScope, IServiceProvider
        {
            private readonly Func<Type, object> _resolve;

            public FakeServiceScopeFactory(Func<Type, object> resolve)
            {
                _resolve = resolve;
            }

            public IServiceScope CreateScope()
            {
                return this;
            }

            public void Dispose()
            {
            }

            public IServiceProvider ServiceProvider => this;

            public object GetService(Type serviceType)
            {
                return _resolve(serviceType);
            }
        }
    }
}