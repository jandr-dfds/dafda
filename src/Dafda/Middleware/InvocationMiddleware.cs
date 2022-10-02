using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Middleware;

internal class InvocationMiddleware : IMiddleware<InvocationContext, InvocationMiddleware.IEndOfPipelineContext>
{
    public Task Invoke(InvocationContext context, Func<IEndOfPipelineContext, Task> next)
    {
        return context.MessageHandler.Invoke(
            context.MessageHandlerInstance,
            context.Message.Instance,
            new MessageHandlerContext(context.Message.Metadata)
        );
    }

    public interface IEndOfPipelineContext : IMiddlewareContext
    {
    }
}