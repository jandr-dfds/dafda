using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dafda.Middleware;

internal class Pipeline
{
    private readonly IMiddleware[] _middlewares;

    public Pipeline(params IMiddleware[] middlewares)
    {
        _middlewares = middlewares;
    }

    public Task Invoke<TContext>(TContext startingContext)
    {
        var pipeline = CreateMiddlewarePipelineDelegate<TContext>();

        if (pipeline == null)
        {
            return Task.CompletedTask;
        }

        return pipeline(startingContext);

    }

    private Func<TContext, Task> CreateMiddlewarePipelineDelegate<TContext>()
    {
        Delegate previous = null;
        var lastIndex = _middlewares.Length - 1;

        for (var index = lastIndex; index >= 0; --index)
        {
            var middleware = _middlewares[index];
            var middlewareDescription = MiddlewareDescription.Describe(middleware);

            if (index == lastIndex)
            {
                previous = CreateEndOfPipelineDelegate(middlewareDescription);
            }
            else
            {
                previous = CreatePipelineDelegate(middlewareDescription, previous);
            }
        }

        return (Func<TContext, Task>)previous;
    }

    private static Delegate CreateEndOfPipelineDelegate(MiddlewareDescription middlewareDescription)
    {
        var inContextType = middlewareDescription.OutContextType;

        var doneDelegate = CreateCompletedTaskDelegate(inContextType);

        return CreatePipelineDelegate(middlewareDescription, doneDelegate);
    }

    private static Delegate CreateCompletedTaskDelegate(Type inContextType)
    {
        var delegateType = typeof(Func<,>).MakeGenericType(inContextType, typeof(Task));
        var parameter = Expression.Parameter(inContextType, "context");
        var body = Expression.Constant(Task.CompletedTask);
        var lambda = Expression.Lambda(delegateType, body, parameter);
        return lambda.Compile();
    }

    private static Delegate CreatePipelineDelegate(MiddlewareDescription middlewareDescription, Delegate previousDelegate)
    {
        var instance = Expression.Constant(middlewareDescription.Instance);
        var context = Expression.Parameter(middlewareDescription.InContextType, "context");
        var previous = Expression.Constant(previousDelegate);
        var call = Expression.Call(instance, middlewareDescription.InvokeMethod, context, previous);
        var lambda = Expression.Lambda(call, context);
        return lambda.Compile();
    }
}