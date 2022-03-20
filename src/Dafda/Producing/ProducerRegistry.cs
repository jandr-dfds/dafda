using System;
using System.Collections.Generic;
using Dafda.Configuration;
using Microsoft.Extensions.Logging;

namespace Dafda.Producing
{
    internal sealed class ProducerRegistry : IDisposable
    {
        private readonly Dictionary<string, ProducerFactory> _registrations = new();

        internal void ConfigureProducerFor<TClient>(ProducerConfiguration configuration, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            var producerName = GetKeyNameOf<TClient>();
            ConfigureProducer(producerName, configuration, outgoingMessageRegistry);
        }

        internal static string GetKeyNameOf<TClient>() => $"__INTERNAL__FOR_CLIENT__{typeof(TClient).FullName}";

        internal void ConfigureProducer(string producerName, ProducerConfiguration configuration, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            if (_registrations.ContainsKey(producerName))
            {
                throw new ProducerFactoryException($"A producer with the name \"{producerName}\" has already been configured. Producer names should be unique.");
            }

            _registrations.Add(producerName, new ProducerFactory(producerName, configuration, outgoingMessageRegistry));
        }

        public Producer Get(string producerName, ILoggerFactory loggerFactory) 
        {
            if (_registrations.TryGetValue(producerName, out var factory))
            {
                return factory.Create(loggerFactory);
            }

            return null;
        }

        public Producer GetFor<TClient>(ILoggerFactory loggerFactory)
        {
            var producerName = GetKeyNameOf<TClient>();
            return Get(producerName, loggerFactory);
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
            private readonly Func<ILoggerFactory, KafkaProducer> _kafkaProducerFactory;
            private readonly MessageIdGenerator _messageIdGenerator;
            private readonly OutgoingMessageRegistry _messageRegistry;

            private KafkaProducer _kafkaProducer;

            public ProducerFactory(string producerName, ProducerConfiguration configuration, OutgoingMessageRegistry messageRegistry)
            {
                _producerName = producerName;
                _kafkaProducerFactory = configuration.KafkaProducerFactory;
                _messageIdGenerator = configuration.MessageIdGenerator;
                _messageRegistry = messageRegistry;
            }

            public Producer Create(ILoggerFactory loggerFactory)
            {
                _kafkaProducer ??= _kafkaProducerFactory(loggerFactory);

                var producer = new Producer(_kafkaProducer, _messageRegistry, _messageIdGenerator)
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