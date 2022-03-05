using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerScope : ConsumerScope
    {
        private readonly ILogger<KafkaConsumerScope> _logger;
        private readonly IConsumer<string, byte[]> _innerKafkaConsumer;

        internal KafkaConsumerScope(ILoggerFactory loggerFactory, IConsumer<string, byte[]> innerKafkaConsumer)
        {
            _logger = loggerFactory.CreateLogger<KafkaConsumerScope>();
            _innerKafkaConsumer = innerKafkaConsumer;
        }

        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);

            _logger.LogDebug("Received message {Key}: {RawMessage}", innerResult.Message?.Key, Encoding.UTF8.GetString(innerResult.Message?.Value ?? Array.Empty<byte>()));

            var result = CreateMessageResult(innerResult);

            return Task.FromResult(result);
        }

        private MessageResult CreateMessageResult(ConsumeResult<string, byte[]> innerResult)
        {
            var result = new MessageResult(
                message: CreateRawMessage(innerResult),
                onCommit: () => OnCommit(innerResult));
            return result;
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
            _innerKafkaConsumer.Commit(innerResult);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _innerKafkaConsumer.Close();
            _innerKafkaConsumer.Dispose();
        }
    }
}