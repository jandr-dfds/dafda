using System;
using Dafda.Middleware;
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

            services.AddTransient(provider => new OutboxQueue(
                configuration.MessageIdGenerator,
                configuration.OutgoingMessageRegistry,
                provider.GetRequiredService<IOutboxEntryRepository>(),
                configuration.Notifier,
                configuration.TopicPayloadSerializerRegistry
            ));
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
                var middleware = configuration.MiddlewareBuilder.Build(provider);
                var pipeline = new Pipeline(middleware);
                var producer = new OutboxProducer(pipeline);
                var outboxDispatcher = new OutboxDispatcher(loggerFactory, outboxUnitOfWorkFactory, producer);

                return new OutboxDispatcherHostedService(outboxListener, outboxDispatcher);
            });
        }
    }
}