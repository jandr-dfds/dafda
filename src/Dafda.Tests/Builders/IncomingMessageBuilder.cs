using System;
using System.Collections.Generic;
using Dafda.Consuming;

namespace Dafda.Tests.Builders;

internal class IncomingMessageBuilder
{
    private readonly Dictionary<string, string> _metadata = new();

    private Type _messageType;
    private object _instance;

    public IncomingMessageBuilder WithMessage<T>(T message)
    {
        _messageType = typeof(T);
        _instance = message;
        return this;
    }

    public IncomingMessageBuilder WithMetadata(string key, string value)
    {
        _metadata[key] = value;
        return this;
    }

    public IncomingMessage Build()
    {
        return new IncomingMessage(_messageType, new Metadata(_metadata), _instance);
    }

    public static implicit operator IncomingMessage(IncomingMessageBuilder builder)
    {
        return builder.Build();
    }
}