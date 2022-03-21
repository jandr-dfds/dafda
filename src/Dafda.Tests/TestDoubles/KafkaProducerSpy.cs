using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Producing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.TestDoubles
{
    internal class KafkaProducerSpy : KafkaProducer
    {
        public KafkaProducerSpy()
            : this(Enumerable.Empty<KeyValuePair<string, string>>())
        {
        }

        private KafkaProducerSpy(IEnumerable<KeyValuePair<string, string>> configuration)
            : base(NullLoggerFactory.Instance, configuration)
        {
        }

        public override Task Produce(OutgoingRawMessage message)
        {
            Topic = message.Topic;
            Key = message.Key;
            Value = message.Data;

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }

        public bool WasDisposed { get; private set; }

        public string Topic { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}