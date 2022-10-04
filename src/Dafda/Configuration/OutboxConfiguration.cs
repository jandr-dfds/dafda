using Dafda.Middleware;
using Dafda.Outbox;

namespace Dafda.Configuration
{
    internal class OutboxConfiguration
    {
        public OutboxConfiguration(IOutboxNotifier notifier, Pipeline pipeline)
        {
            Notifier = notifier;
            Pipeline = pipeline;
        }

        public IOutboxNotifier Notifier { get; }
        public Pipeline Pipeline { get; }
    }
}