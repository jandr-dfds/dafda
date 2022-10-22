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
        private readonly Pipeline _pipeline;
        private readonly ConsumerErrorHandler _errorHandler;
        private readonly bool _isAutoCommitEnabled;

        public Consumer(
            ILogger<Consumer> logger,
            Func<ConsumerScope> consumerScopeFactory,
            IServiceScopeFactory serviceScopeFactory,
            Pipeline pipeline,
            ConsumerErrorHandler errorHandler,
            bool isAutoCommitEnabled = false)
        {
            _logger = logger;
            _consumerScopeFactory = consumerScopeFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _pipeline = pipeline;
            _errorHandler = errorHandler;
            _isAutoCommitEnabled = isAutoCommitEnabled;
        }

        public async Task Consume(Cancelable cancelable)
        {
            while (await ConsumerLoop(cancelable))
            {
            }
        }

        private async Task<bool> ConsumerLoop(Cancelable cancelable)
        {
            try
            {
                using var consumerScope = _consumerScopeFactory();

                while (!cancelable.IsCancelled)
                {
                    await consumerScope.Consume(OnMessage, cancelable.Token);
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled error occurred while consuming");

                var errorHandled = await _errorHandler.HandleError(exception);
                if (errorHandled)
                {
                    return true;
                }
                throw;
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