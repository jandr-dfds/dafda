using Dafda.Producing;

namespace Dafda.Middleware
{
    internal class OutboxMessageContext : IMiddlewareContext
    {
        public OutboxMessageContext(OutgoingMessage[] messages)
        {
            Messages = messages;
        }

        public OutgoingMessage[] Messages { get; }
    }
}