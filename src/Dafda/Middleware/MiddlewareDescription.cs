using System;
using System.Linq;
using System.Reflection;

namespace Dafda.Middleware;

internal class MiddlewareDescription
{
    public static MiddlewareDescription Describe(IMiddleware middleware)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        var concreteType = middleware.GetType();

        var interfaceType = concreteType
            .GetInterfaces()
            .FirstOrDefault(ClosesOpenMiddlewareType);

        if (interfaceType == null)
        {
            throw new InvalidOperationException("Middleware must implement IMiddleware<TInContext, TOutContext>");
        }

        var method = concreteType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
        if (method == null)
        {
            throw new InvalidOperationException("Middleware must provide an Invoke method");
        }

        return new MiddlewareDescription(middleware, interfaceType, method);
    }

    private static bool ClosesOpenMiddlewareType(Type candidateType)
    {
        return candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == typeof(IMiddleware<,>);
    }

    private MiddlewareDescription(IMiddleware instance, Type baseType, MethodInfo invokeMethod)
    {
        Instance = instance;
        InContextType = baseType.GetGenericArguments()[0];
        OutContextType = baseType.GetGenericArguments()[1];
        InvokeMethod = invokeMethod;
    }

    public IMiddleware Instance { get; }
    public Type InContextType { get; }
    public Type OutContextType { get; }
    public MethodInfo InvokeMethod { get; }
}