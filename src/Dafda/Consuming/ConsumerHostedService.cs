using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class ConsumerHostedService : BackgroundService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly Consumer _consumer;
        private readonly string _groupId;
        private readonly ConsumerErrorHandler _consumerErrorHandler;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IHostApplicationLifetime applicationLifetime, Consumer consumer, string groupId, ConsumerErrorHandler consumerErrorHandler)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _consumer = consumer;
            _groupId = groupId;
            _consumerErrorHandler = consumerErrorHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConsumeLoop(Until.CancelledBy(stoppingToken));
        }

        public async Task ConsumeLoop(Cancelable cancelable)
        {
            while (true)
            {
                try
                {
                    _logger.LogDebug("ConsumerHostedService [{GroupId}] started", _groupId);
                    await _consumer.Consume(cancelable);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("ConsumerHostedService [{GroupId}] cancelled", _groupId);
                    break;
                }
                catch (Exception err)
                {
                    _logger.LogError(err, "Unhandled error occurred while consuming messaging");
                    var failureStrategy = await _consumerErrorHandler.Handle(err);
                    if (failureStrategy == ConsumerFailureStrategy.Default)
                    {
                        _applicationLifetime.StopApplication();
                        break;
                    }
                }
            }
        }
    }
}