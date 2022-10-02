using System;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary></summary>
    public static class ConsumerServiceCollectionExtensions
    {
        /// <summary>
        /// Add a Kafka consumer. The consumer will run in a <see cref="IHostedService"/>.
        /// It is possible to configure multi consumers.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddConsumer(this IServiceCollection services, Action<ConsumerOptions> options = null)
        {
            var consumerOptions = new ConsumerOptions(services);
            options?.Invoke(consumerOptions);
            var configuration = consumerOptions.Build();

            UniqueConsumerGroupId.Ensure(services, configuration.GroupId);

            ConsumerHostedService CreateConsumerHostedService(IServiceProvider provider)
            {
                var consumer = new Consumer(
                    provider.GetRequiredService<ILogger<Consumer>>(),
                    () => configuration.ConsumerScopeFactory(provider),
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    configuration.MiddlewareBuilder,
                    configuration.EnableAutoCommit
                );

                return new ConsumerHostedService(
                logger: provider.GetRequiredService<ILogger<ConsumerHostedService>>(),
                applicationLifetime: provider.GetRequiredService<IHostApplicationLifetime>(),
                    consumer: consumer,
                configuration.GroupId,
                configuration.ConsumerErrorHandler
            );
            }

            services.AddTransient<IHostedService, ConsumerHostedService>(CreateConsumerHostedService);
            services.AddTransient<ConsumerHostedService>(CreateConsumerHostedService); // NOTE: [jandr] is this needed?
        }
    }
}
