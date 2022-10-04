using System;
using System.Threading.Tasks;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class Consumer
    {
        private readonly ILogger<Consumer> _logger;
        private readonly Func<ConsumerScope> _consumerScopeFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly bool _isAutoCommitEnabled;
        private readonly Pipeline _pipeline;

        public Consumer(
            ILogger<Consumer> logger,
            Func<ConsumerScope> consumerScopeFactory,
            IServiceScopeFactory serviceScopeFactory,
            Pipeline pipeline,
            bool isAutoCommitEnabled = false)
        {
            _logger = logger;
            _consumerScopeFactory = consumerScopeFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _pipeline = pipeline;
            _isAutoCommitEnabled = isAutoCommitEnabled;
        }

        public async Task Consume(Cancelable cancelable)
        {
            using (var consumerScope = _consumerScopeFactory())
            {
                while (!cancelable.IsCancelled)
                {
                    await consumerScope.Consume(OnMessage, cancelable.Token);
                }
            }
        }

        private async Task OnMessage(MessageResult messageResult)
        {
            _logger.LogDebug("MessageScope:Begin");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _logger.LogDebug("UnitOfWork:Begin");

                await _pipeline.Invoke(new IncomingRawMessageContext(messageResult.Message, new RootMiddlewareContext(scope.ServiceProvider)));

                _logger.LogDebug("UnitOfWork:End");
            }

            if (!_isAutoCommitEnabled)
            {
                await messageResult.Commit();
            }

            _logger.LogDebug("MessageScope:End");
        }
    }
}