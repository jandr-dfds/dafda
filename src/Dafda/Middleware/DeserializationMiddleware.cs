using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Middleware;

internal class DeserializationMiddleware : IMiddleware<IncomingRawMessageContext, IncomingMessageContext>
{
    private readonly IDeserializer _deserializer;

    public DeserializationMiddleware(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }

    public Task Invoke(IncomingRawMessageContext context, Func<IncomingMessageContext, Task> next)
    {
        var incomingMessage = _deserializer.Deserialize(context.Message);
        return next(new IncomingMessageContext(incomingMessage, context));
    }
}