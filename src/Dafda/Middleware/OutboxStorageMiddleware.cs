using System;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Middleware;

internal class OutboxStorageMiddleware : IMiddleware<OutboxStorageContext, OutboxStorageMiddleware.IEndOfPipelineContext>
{
    private readonly IOutboxEntryRepository _repository;

    public OutboxStorageMiddleware(IOutboxEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task Invoke(OutboxStorageContext context, Func<IEndOfPipelineContext, Task> next)
    {
        await _repository.Add(context.OutboxEntries);
    }

    public interface IEndOfPipelineContext : IMiddlewareContext
    {
    }
}