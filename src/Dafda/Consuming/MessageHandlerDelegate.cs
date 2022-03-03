using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dafda.Consuming;

/// <summary>
/// 
/// </summary>
public sealed class MessageHandlerDelegate
{
    internal static MessageHandlerDelegate Create<TMessage, TMessageHandler>()
        where TMessageHandler : IMessageHandler<TMessage>
    {
        return Create(typeof(TMessage), typeof(TMessageHandler));
    }

    internal static MessageHandlerDelegate Create(Type messageType, Type messageHandlerType)
    {
        var interfaceType = typeof(IMessageHandler<>).MakeGenericType(messageType);

        if (!interfaceType.IsAssignableFrom(messageHandlerType))
        {
            throw new MessageRegistrationException($"Type {messageHandlerType.Name} must implement IMessageHandler<{messageType.Name}>");
        }

        var methodInfo = messageHandlerType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
        if (methodInfo == null)
        {
            throw new MessageRegistrationException($"Type {messageHandlerType.Name} must implement IMessageHandler<{messageType.Name}>");
        }

        var instanceParam = Expression.Parameter(typeof(object));
        var castInstance = Expression.Convert(instanceParam, messageHandlerType);
        var messageParam = Expression.Parameter(typeof(object));
        var messageCastParam = Expression.Convert(messageParam, messageType);
        var contextParam = Expression.Parameter(typeof(MessageHandlerContext));

        var call = Expression.Call(castInstance, methodInfo, messageCastParam, contextParam);

        var invocation = Expression
            .Lambda<Func<object, object, MessageHandlerContext, Task>>(call, instanceParam, messageParam, contextParam)
            .Compile();

        return new MessageHandlerDelegate(invocation, messageHandlerType);
    }

    private readonly Func<object, object, MessageHandlerContext, Task> _invocation;

    private MessageHandlerDelegate(Func<object, object, MessageHandlerContext, Task> invocation, Type handlerType)
    {
        _invocation = invocation;
        HandlerType = handlerType;
    }

    /// <summary>
    /// 
    /// </summary>
    public Type HandlerType { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="message"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task Invoke(object instance, object message, MessageHandlerContext context)
    {
        return _invocation(instance, message, context);
    }
}