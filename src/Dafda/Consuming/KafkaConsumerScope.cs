using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal sealed class KafkaConsumerScope : ConsumerScope
    {
        private readonly ILogger<KafkaConsumerScope> _logger;
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        private readonly IEnumerable<string> _topics;
        private readonly bool _readFromBeginning;

        private IConsumer<string, byte[]> _consumer;

        public KafkaConsumerScope(
            ILogger<KafkaConsumerScope> logger,
            IEnumerable<KeyValuePair<string, string>> configuration,
            IEnumerable<string> topics,
            bool readFromBeginning)
        {
            _logger = logger;
            _configuration = configuration;
            _topics = topics;
            _readFromBeginning = readFromBeginning;
        }

        public override async Task Consume(Func<MessageResult, Task> onMessageCallback, CancellationToken cancellationToken)
        {
            EnsureConsumer();

            var messageResult = await Consume(cancellationToken);

            await onMessageCallback(messageResult);
        }

        private void EnsureConsumer()
        {
            if (_consumer != null)
            {
                return;
            }

            var consumerBuilder = new ConsumerBuilder<string, byte[]>(_configuration);
            if (_readFromBeginning)
            {
                consumerBuilder.SetPartitionsAssignedHandler((_, topicPartitions) => {return topicPartitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning));});
            }

            _consumer = consumerBuilder.Build();
            _consumer.Subscribe(_topics);
        }

        private Task<MessageResult> Consume(CancellationToken cancellationToken)
        {
            var innerResult = _consumer.Consume(cancellationToken);

            _logger.LogDebug("Received message {Key}: {RawMessage}", innerResult.Message?.Key, Encoding.UTF8.GetString(innerResult.Message?.Value ?? Array.Empty<byte>()));

            var result = CreateMessageResult(innerResult);

            return Task.FromResult(result);
        }

        private MessageResult CreateMessageResult(ConsumeResult<string, byte[]> innerResult)
        {
            return new MessageResult(
                message: CreateRawMessage(innerResult),
                onCommit: () => OnCommit(innerResult));
        }

        private static RawMessage CreateRawMessage(ConsumeResult<string, byte[]> consumeResult)
        {
            var message = consumeResult.Message;
            var headers = new Dictionary<string, string>();

            foreach (var header in message.Headers)
            {
                headers[header.Key] = Encoding.UTF8.GetString(header.GetValueBytes());
            }

            return new RawMessage(message.Key, headers, message.Value);
        }

        private Task OnCommit(ConsumeResult<string, byte[]> innerResult)
        {
            _consumer.Commit(innerResult);
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _consumer.Close();
                _consumer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}