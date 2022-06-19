using System.Threading.Tasks;
using Academia.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Academia.Controllers;

public static class TransactionalMiddlewareExtensions
{
    public static void UseTransactionalMiddleware(this WebApplication webApplication)
    {
        webApplication.UseMiddleware<TransactionalMiddleware>();
    }

    public static RouteHandlerBuilder Transactional(this RouteHandlerBuilder builder)
    {
        return builder.WithMetadata(new TransactionalMetadata());
    }

    private class TransactionalMetadata
    {
    }

    private class TransactionalMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionalMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.GetEndpoint()?.Metadata.GetMetadata<TransactionalMetadata>() != null)
            {
                var _transactionalOutbox = httpContext.RequestServices.GetRequiredService<ITransactionalOutbox>();
                await _transactionalOutbox.Execute(async () => await _next(httpContext));
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}