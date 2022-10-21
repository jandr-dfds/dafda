using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class ConsumerHostedService : BackgroundService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly Consumer _consumer;
        private readonly ConsumerErrorHandler _consumerErrorHandler;
        private readonly string _groupId;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, Consumer consumer, ConsumerErrorHandler errorHandler, string groupId)
        {
            _logger = logger;
            _consumer = consumer;
            _consumerErrorHandler = errorHandler;
            _groupId = groupId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConsumeLoop(Until.CancelledBy(stoppingToken));
        }

        public async Task ConsumeLoop(Cancelable cancelable)
        {
            while (await Consume(cancelable))
            {
            }
        }

        private async Task<bool> Consume(Cancelable cancelable)
        {
            try
            {
                _logger.LogDebug("ConsumerHostedService [{GroupId}] started", _groupId);
                await _consumer.Consume(cancelable);
                return false;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("ConsumerHostedService [{GroupId}] cancelled", _groupId);
                return false;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Unhandled error occurred while consuming messaging");
                return await _consumerErrorHandler.HandleError(err);
            }
        }
    }
}