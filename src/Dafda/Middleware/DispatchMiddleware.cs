using System;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Middleware;

internal class DispatchMiddleware : IMiddleware<OutgoingRawMessageContext, DispatchMiddleware.IEndOfPipelineContext>
{
    public Task Invoke(OutgoingRawMessageContext context, Func<IEndOfPipelineContext, Task> next)
    {
        var kafkaProducer = context.Get<KafkaProducer>();

        return kafkaProducer.Produce(context.Message);
    }

    public interface IEndOfPipelineContext : IMiddlewareContext
    {
    }
}