using System;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Middleware;

internal class OutboxStorageMiddleware : IMiddleware<OutboxStorageContext, OutboxStorageMiddleware.IEndOfPipelineContext>
{
    public async Task Invoke(OutboxStorageContext context, Func<IEndOfPipelineContext, Task> next)
    {
        var repository = context.ServiceProvider.GetRequiredService<IOutboxEntryRepository>();
        await repository.Add(context.OutboxEntries);
    }

    public interface IEndOfPipelineContext : IMiddlewareContext
    {
    }
}