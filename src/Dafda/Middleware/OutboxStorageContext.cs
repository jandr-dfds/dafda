using Dafda.Outbox;

namespace Dafda.Middleware;

internal class OutboxStorageContext : MiddlewareContext
{
    public OutboxStorageContext(OutboxEntry[] outboxEntries, IMiddlewareContext parent)
        : base(parent)
    {
        OutboxEntries = outboxEntries;
    }

    public OutboxEntry[] OutboxEntries { get; }
}