using System;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
{
    /// <summary></summary>
    public static class ProducerServiceCollectionExtensions
    {
        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>. 
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TService, TImplementation>(this IServiceCollection services, Action<ProducerOptions> options)
            where TImplementation : class, TService
            where TService : class
        {
            var registry = RegisterProducer<TImplementation>(services, options);
            services.AddTransient<TService, TImplementation>(provider => CreateInstance<TImplementation>(provider, registry));
        }

        private static ProducerRegistry RegisterProducer<TImplementation>(IServiceCollection services, Action<ProducerOptions> options)
        {
            var consumerOptions = new ProducerOptions(services);
            options?.Invoke(consumerOptions);

            var producerConfiguration = consumerOptions.Build();

            var registry = services.GetOrAddSingleton(() => new ProducerRegistry());
            registry.ConfigureProducerFor<TImplementation>(producerConfiguration);
            return registry;
        }

        private static TImplementation CreateInstance<TImplementation>(IServiceProvider provider, ProducerRegistry registry)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var producer = registry.GetFor<TImplementation>(loggerFactory);
            return ActivatorUtilities.CreateInstance<TImplementation>(provider, producer);
        }

        /// <summary>
        /// Add a Kafka producer available through the Microsoft dependency injection's <see cref="IServiceProvider"/>
        /// as <see cref="Producer"/>. 
        ///
        /// NOTE: currently only a single producer can be configured per <typeparamref name="TClient"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used in <c>Startup</c>.</param>
        /// <param name="options">Use this action to override Dafda and underlying Kafka configuration.</param>
        public static void AddProducerFor<TClient>(this IServiceCollection services, Action<ProducerOptions> options) where TClient : class
        {
            var registry = RegisterProducer<TClient>(services, options);
            services.AddTransient(provider => CreateInstance<TClient>(provider, registry));
        }
    }
}