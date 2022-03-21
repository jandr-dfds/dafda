using System;
using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Middleware;

internal class SerializationMiddleware : IMiddleware<PayloadDescriptorContext, OutgoingRawMessageContext>
{
    private readonly TopicPayloadSerializerRegistry _payloadSerializerRegistry;

    public SerializationMiddleware(TopicPayloadSerializerRegistry payloadSerializerRegistry)
    {
        _payloadSerializerRegistry = payloadSerializerRegistry;
    }

    public async Task Invoke(PayloadDescriptorContext context, Func<OutgoingRawMessageContext, Task> next)
    {
        var payloadDescriptor = context.PayloadDescriptor;
        var serializer = _payloadSerializerRegistry.Get(payloadDescriptor.TopicName);

        var message = new OutgoingRawMessage(
            payloadDescriptor.TopicName,
            payloadDescriptor.PartitionKey,
            await serializer.Serialize(payloadDescriptor)
        );

        await next(new OutgoingRawMessageContext(message));
    }
}