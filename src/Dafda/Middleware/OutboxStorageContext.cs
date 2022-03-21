using Dafda.Outbox;

namespace Dafda.Middleware;

internal class OutboxStorageContext : IMiddlewareContext
{
    public OutboxStorageContext(OutboxEntry[] outboxEntries)
    {
        OutboxEntries = outboxEntries;
    }

    public OutboxEntry[] OutboxEntries { get; }
}