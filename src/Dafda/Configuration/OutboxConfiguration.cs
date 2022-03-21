using Dafda.Middleware;
using Dafda.Outbox;

namespace Dafda.Configuration
{
    internal class OutboxConfiguration
    {
        public OutboxConfiguration(IOutboxNotifier notifier, MiddlewareBuilder<OutboxMessageContext> middlewareBuilder)
        {
            Notifier = notifier;
            MiddlewareBuilder = middlewareBuilder;
        }

        public IOutboxNotifier Notifier { get; }
        public MiddlewareBuilder<OutboxMessageContext> MiddlewareBuilder { get; }
    }
}