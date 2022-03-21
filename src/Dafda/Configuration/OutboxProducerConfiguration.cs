using Dafda.Middleware;

namespace Dafda.Configuration
{
    internal class OutboxProducerConfiguration
    {
        public OutboxProducerConfiguration(MiddlewareBuilder<OutgoingRawMessageContext> middlewareBuilder)
        {
            MiddlewareBuilder = middlewareBuilder;
        }

        public MiddlewareBuilder<OutgoingRawMessageContext> MiddlewareBuilder { get; }
    }
}