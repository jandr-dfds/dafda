using System;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Middleware;

internal class PayloadDescriptionMiddleware : IMiddleware<OutgoingMessageContext, PayloadDescriptorContext>
{
    private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

    internal PayloadDescriptionMiddleware(OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
    {
        _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
    }

    public Task Invoke(OutgoingMessageContext context, Func<PayloadDescriptorContext, Task> next)
    {
        var payloadDescriptor = _payloadDescriptorFactory.Create(context.Message, context.Metadata);

        return next(new PayloadDescriptorContext(payloadDescriptor));
    }
}