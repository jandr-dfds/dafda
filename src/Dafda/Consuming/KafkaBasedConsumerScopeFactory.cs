using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class KafkaBasedConsumerScopeFactory : IConsumerScopeFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        private readonly IEnumerable<string> _topics;
        private readonly bool _readFromBeginning;

        public KafkaBasedConsumerScopeFactory(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, IEnumerable<string> topics, bool readFromBeginning)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _topics = topics;
            _readFromBeginning = readFromBeginning;
        }
        
        public ConsumerScope CreateConsumerScope()
        {
            var consumerBuilder = new ConsumerBuilder<string, byte[]>(_configuration);
            if (_readFromBeginning)
            {
                consumerBuilder.SetPartitionsAssignedHandler((_, topicPartitions) => { return topicPartitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)); });
            }

            var consumer = consumerBuilder.Build();
            consumer.Subscribe(_topics);

            return new KafkaConsumerScope(_loggerFactory, consumer);
        }
    }
}