using System;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Middleware;

internal class DispatchMiddleware : IMiddleware<OutgoingRawMessageContext, DispatchMiddleware.IEndOfPipelineContext>
{
    private readonly KafkaProducer _kafkaProducer;

    public DispatchMiddleware(KafkaProducer kafkaProducer)
    {
        _kafkaProducer = kafkaProducer;
    }

    public Task Invoke(OutgoingRawMessageContext context, Func<IEndOfPipelineContext, Task> next)
    {
        return _kafkaProducer.Produce(context.Message);
    }

    public interface IEndOfPipelineContext : IMiddlewareContext
    {
    }
}