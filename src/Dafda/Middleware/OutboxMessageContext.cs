using Dafda.Producing;

namespace Dafda.Middleware
{
    internal class OutboxMessageContext : MiddlewareContext
    {
        public OutboxMessageContext(OutgoingMessage[] messages, IMiddlewareContext parent)
            : base(parent)
        {
            Messages = messages;
        }

        public OutgoingMessage[] Messages { get; }
    }
}