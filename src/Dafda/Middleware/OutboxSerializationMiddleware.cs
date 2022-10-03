using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Serializing;

namespace Dafda.Middleware;

internal class OutboxSerializationMiddleware : IMiddleware<OutboxPayloadDescriptionContext, OutboxStorageContext>
{
    private readonly TopicPayloadSerializerRegistry _payloadSerializerRegistry;

    public OutboxSerializationMiddleware(TopicPayloadSerializerRegistry payloadSerializerRegistry)
    {
        _payloadSerializerRegistry = payloadSerializerRegistry;
    }

    public async Task Invoke(OutboxPayloadDescriptionContext context, Func<OutboxStorageContext, Task> next)
    {
        var outboxEntries = new List<OutboxEntry>();

        foreach (var payloadDescriptor in context.PayloadDescriptors)
        {
            var serializer = _payloadSerializerRegistry.Get(payloadDescriptor.TopicName);

            var data = await serializer.Serialize(payloadDescriptor);

            var outboxEntry = new OutboxEntry(
                messageId: Guid.Parse(payloadDescriptor.MessageId),
                topic: payloadDescriptor.TopicName,
                key: payloadDescriptor.PartitionKey,
                payload: data,
                occurredUtc: DateTime.UtcNow
            );

            outboxEntries.Add(outboxEntry);
        }

        await next(new OutboxStorageContext(outboxEntries.ToArray(), context));
    }
}