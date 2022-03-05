using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class Consumer
    {
        private readonly ILogger<Consumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConsumerScopeFactory _consumerScopeFactory;
        private readonly MiddlewareBuilder<IncomingRawMessageContext> _middlewareBuilder;
        private readonly bool _isAutoCommitEnabled;

        public Consumer(
            ILogger<Consumer> logger,
            IConsumerScopeFactory consumerScopeFactory,
            IServiceScopeFactory serviceScopeFactory, 
            MiddlewareBuilder<IncomingRawMessageContext> middlewareBuilder,
            bool isAutoCommitEnabled = false)
        {
            _logger = logger;
            _consumerScopeFactory = consumerScopeFactory ?? throw new ArgumentNullException(nameof(consumerScopeFactory));
            _serviceScopeFactory = serviceScopeFactory;
            _middlewareBuilder = middlewareBuilder;
            _isAutoCommitEnabled = isAutoCommitEnabled;
        }

        public async Task Consume(CancellationToken cancellationToken)
        {
            using (var consumerScope = _consumerScopeFactory.CreateConsumerScope())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(consumerScope, cancellationToken);
                }
            }
        }

        private async Task ProcessNextMessage(ConsumerScope consumerScope, CancellationToken cancellationToken)
        {
            var messageResult = await consumerScope.GetNext(cancellationToken);
            _logger.LogDebug("TransactionScope:Begin");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _logger.LogDebug("UnitOfWork:Begin");
                var scopedServiceProvider = scope.ServiceProvider;

                // initialize pipeline
                var middlewares = _middlewareBuilder.Build(scopedServiceProvider);
                var pipeline = new Pipeline(middlewares);

                // execute pipeline
                await pipeline.Invoke(new IncomingRawMessageContext(messageResult.Message));

                _logger.LogDebug("UnitOfWork:End");
            }

            if (!_isAutoCommitEnabled)
            {
                await messageResult.Commit();
            }
            _logger.LogDebug("TransactionScope:End");
        }
    }
}
