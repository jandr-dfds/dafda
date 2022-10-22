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
        private readonly string _groupId;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, Consumer consumer, string groupId)
        {
            _logger = logger;
            _consumer = consumer;
            _groupId = groupId;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("ConsumerHostedService [{GroupId}] started", _groupId);

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("ConsumerHostedService [{GroupId}] stopped", _groupId);

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.Consume(Until.CancelledBy(stoppingToken));
        }
    }
}