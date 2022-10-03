using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Dafda.Producing;

namespace Dafda.Outbox
{
    /// <summary></summary>
    public sealed class OutboxQueue
    {
        private readonly IOutboxNotifier _outboxNotifier;
        private readonly Pipeline _pipeline;

        internal OutboxQueue(IOutboxNotifier outboxNotifier, Pipeline pipeline)
        {
            _outboxNotifier = outboxNotifier;
            _pipeline = pipeline;
        }

        /// <summary>
        /// Send domain events to be processed by the Dafda outbox feature
        /// </summary>
        /// <param name="messages">The list of messages to add to the outbox</param>
        /// <returns>
        /// A <see cref="IOutboxNotifier"/> which can be used to signal the outbox processing mechanism,
        /// whether local or remote. The <see cref="IOutboxNotifier.Notify"/> can be used to signal the
        /// processor when new events are available.
        /// </returns>
        /// <remarks>
        /// Calling <see cref="IOutboxNotifier.Notify"/> can happen as part of a transaction, e.g. when
        /// using Postgres' <c>LISTEN/NOTIFY</c>, or after the transactions has been committed, when using
        /// the built-in <see cref="IOutboxNotifier"/>.
        /// </remarks>
        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages)
        {
            var metadata = new Metadata();

            return await Enqueue(messages, metadata);
        }

        /// <summary>
        /// Send domain events to be processed by the Dafda outbox feature
        /// </summary>
        /// <param name="messages">The list of messages to add to the outbox</param>
        /// <param name="headers">The message headers</param>
        /// <returns>
        /// A <see cref="IOutboxNotifier"/> which can be used to signal the outbox processing mechanism,
        /// whether local or remote. The <see cref="IOutboxNotifier.Notify"/> can be used to signal the
        /// processor when new events are available.
        /// </returns>
        /// <remarks>
        /// Calling <see cref="IOutboxNotifier.Notify"/> can happen as part of a transaction, e.g. when
        /// using Postgres' <c>LISTEN/NOTIFY</c>, or after the transactions has been committed, when using
        /// the built-in <see cref="IOutboxNotifier"/>.
        /// </remarks>
        public async Task<IOutboxNotifier> Enqueue(IEnumerable<object> messages, Metadata headers)
        {
            var outgoingMessages = messages
                .Select(m => new OutgoingMessage(m, headers))
                .ToArray();

            await _pipeline.Invoke(new OutboxMessageContext(outgoingMessages, new RootMiddlewareContext(null)));

            return _outboxNotifier;
        }
    }
}