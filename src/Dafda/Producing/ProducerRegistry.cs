using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Configuration;
using Dafda.Middleware;

namespace Dafda.Producing
{
    internal sealed class ProducerRegistry : IDisposable
    {
        private readonly Dictionary<string, ProducerFactory> _registrations = new();

        internal void ConfigureProducerFor<TClient>(ProducerConfiguration configuration)
        {
            var producerName = GetKeyNameOf<TClient>();
            ConfigureProducer(producerName, configuration);
        }

        internal static string GetKeyNameOf<TClient>() => $"__INTERNAL__FOR_CLIENT__{typeof(TClient).FullName}";

        internal void ConfigureProducer(string producerName, ProducerConfiguration configuration)
        {
            if (_registrations.ContainsKey(producerName))
            {
                throw new ProducerFactoryException($"A producer with the name \"{producerName}\" has already been configured. Producer names should be unique.");
            }

            _registrations.Add(producerName, new ProducerFactory(producerName, configuration));
        }

        public Producer Get(string producerName, IServiceProvider serviceProvider)
        {
            if (_registrations.TryGetValue(producerName, out var factory))
            {
                return factory.Create(serviceProvider);
            }

            return null;
        }

        public Producer GetFor<TClient>(IServiceProvider serviceProvider)
        {
            var producerName = GetKeyNameOf<TClient>();
            return Get(producerName, serviceProvider);
        }

        public void Dispose()
        {
            foreach (var registration in _registrations.Values)
            {
                registration.Dispose();
            }
        }

        private class ProducerFactory : IDisposable
        {
            private readonly string _producerName;
            private readonly Func<IServiceProvider, KafkaProducer> _kafkaProducerFactory;
            private readonly MiddlewareBuilder<OutgoingMessageContext> _middlewareBuilder;

            private KafkaProducer _kafkaProducer;

            public ProducerFactory(string producerName, ProducerConfiguration configuration)
            {
                _producerName = producerName;
                _kafkaProducerFactory = configuration.KafkaProducerFactory;
                _middlewareBuilder = configuration.MiddlewareBuilder;
            }

            public Producer Create(IServiceProvider provider)
            {
                _kafkaProducer ??= _kafkaProducerFactory(provider);

                var middlewares = _middlewareBuilder
                    .Build(provider)
                    .ToArray();

                var pipeline = new Pipeline(middlewares);

                var producer = new Producer(pipeline, provider, _kafkaProducer)
                {
                    Name = _producerName
                };

                return producer;
            }

            public void Dispose()
            {
                _kafkaProducer?.Dispose();
            }
        }
    }
}