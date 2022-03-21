using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IDisposable
    {
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<string, string> _innerKafkaProducer;

        public KafkaProducer(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration)
        {
            _logger = loggerFactory.CreateLogger<KafkaProducer>();
            _innerKafkaProducer = new ProducerBuilder<string, string>(configuration).Build();
        }

        public virtual async Task Produce(OutgoingRawMessage message)
        {
            try
            {
                _logger.LogDebug("Producing message with {Key} on {Topic}", message.Key, message.Topic);

                await _innerKafkaProducer.ProduceAsync(
                    topic: message.Topic,
                    message: new Message<string, string>
                    {
                        Key = message.Key,
                        Value = message.Data
                    }
                );
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public virtual void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}