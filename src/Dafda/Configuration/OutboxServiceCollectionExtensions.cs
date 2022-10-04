using System;
using Dafda.Outbox;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary></summary>
    public static class OutboxServiceCollectionExtensions
    {
        /// <summary>
        /// Enable the Dafda outbox collector implementation, configurable using the <paramref name="options"/>
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">Configure the <see cref="OutboxOptions"/></param>
        public static void AddOutbox(this IServiceCollection services, Action<OutboxOptions> options)
        {
            var outboxOptions = new OutboxOptions(services);
            options?.Invoke(outboxOptions);
            var configuration = outboxOptions.Build();

            services.AddTransient(provider => new OutboxQueue(configuration.Notifier, configuration.Pipeline, provider));
        }

        /// <summary>
        /// Enable the Dafda outbox producer implementation, configurable using the <paramref name="options"/>
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">Configure the <see cref="OutboxProducerOptions"/></param>
        public static void AddOutboxProducer(this IServiceCollection services, Action<OutboxProducerOptions> options)
        {
            var outboxProducerOptions = new OutboxProducerOptions(services);
            options?.Invoke(outboxProducerOptions);
            var configuration = outboxProducerOptions.Build();

            var outboxListener = outboxProducerOptions.OutboxListener;
            if (outboxListener == null)
            {
                throw new InvalidConfigurationException($"No {nameof(IOutboxListener)} was registered. Please use the {nameof(OutboxProducerOptions.WithListener)} in the {nameof(AddOutboxProducer)} configuration.");
            }

            services.AddTransient<IHostedService, OutboxDispatcherHostedService>(provider =>
            {
                var outboxUnitOfWorkFactory = provider.GetRequiredService<IOutboxUnitOfWorkFactory>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var kafkaProducer = configuration.KafkaProducerFactory(provider);
                var pipeline = configuration.Pipeline;
                var producer = new OutboxProducer(pipeline, provider, kafkaProducer);
                var outboxDispatcher = new OutboxDispatcher(loggerFactory, outboxUnitOfWorkFactory, producer);

                return new OutboxDispatcherHostedService(outboxListener, outboxDispatcher);
            });
        }
    }
}