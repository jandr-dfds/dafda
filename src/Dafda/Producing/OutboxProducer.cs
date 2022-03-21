using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Outbox;

namespace Dafda.Producing
{
    /// <summary>
    /// A specific producer for the Dafda outbox
    /// </summary>
    internal sealed class OutboxProducer
    {
        private readonly Pipeline _pipeline;

        internal OutboxProducer(Pipeline pipeline)
        {
            _pipeline = pipeline;
        }

        /// <summary>
        /// Produce the <see cref="OutboxEntry"/> on Kafka using the data contained
        /// </summary>
        /// <param name="entry">The outbox message</param>
        public async Task Produce(OutboxEntry entry)
        {
            await _pipeline.Invoke(new OutgoingRawMessageContext(new OutgoingRawMessage(entry.Topic, entry.Key, entry.Payload)));
        }
    }
}