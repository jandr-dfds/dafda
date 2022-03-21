using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Middleware;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Builders
{
    internal class OutboxQueueBuilder
    {
        private OutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private IOutboxEntryRepository _outboxEntryRepository = new DummyOutboxEntryRepository();
        private IPayloadSerializer _payloadSerializer = new DefaultPayloadSerializer();

        public OutboxQueueBuilder With(OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public OutboxQueueBuilder With(IOutboxEntryRepository outboxEntryRepository)
        {
            _outboxEntryRepository = outboxEntryRepository;
            return this;
        }

        public OutboxQueue Build()
        {
            var services = new ServiceCollection();
            var middlewareBuilder = new MiddlewareBuilder<OutboxMessageContext>(services);

            middlewareBuilder
                .Register(_ => new OutboxPayloadDescriptionMiddleware(_outgoingMessageRegistry, MessageIdGenerator.Default))
                .Register(_ => new OutboxSerializationMiddleware(new TopicPayloadSerializerRegistry(() => _payloadSerializer)))
                .Register(_ => new OutboxStorageMiddleware(_outboxEntryRepository));

            var serviceProvider = services.BuildServiceProvider();
            var pipeline = new Pipeline(middlewareBuilder.Build(serviceProvider));

            return new OutboxQueue(outboxNotifier: new NullOutboxNotifier(), pipeline);
        }

        public static implicit operator OutboxQueue(OutboxQueueBuilder builder)
        {
            return builder.Build();
        }

        private class DummyOutboxEntryRepository : IOutboxEntryRepository
        {
            public Task Add(IEnumerable<OutboxEntry> outboxEntries)
            {
                return Task.CompletedTask;
            }
        }

        private class NullOutboxNotifier : IOutboxNotifier
        {
            public Task Notify(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}