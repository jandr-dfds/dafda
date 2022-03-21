using System;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Middleware;

internal class OutboxPayloadDescriptionMiddleware : IMiddleware<OutboxMessageContext, OutboxPayloadDescriptionContext>
{
    private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

    internal OutboxPayloadDescriptionMiddleware(OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
    {
        _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
    }

    public Task Invoke(OutboxMessageContext context, Func<OutboxPayloadDescriptionContext, Task> next)
    {
        var payloadDescriptors = context.Messages
            .Select(message => _payloadDescriptorFactory.Create(message))
            .ToArray();

        return next(new OutboxPayloadDescriptionContext(payloadDescriptors));
    }
}