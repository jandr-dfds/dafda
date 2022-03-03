using System;
using System.Collections.Generic;
using System.Text;
using Dafda.Consuming;

namespace Dafda.Tests.Builders;

internal class RawMessageBuilder
{
    private byte[] _data = Array.Empty<byte>();

    public RawMessageBuilder WithData(byte[] data)
    {
        _data = data;
        return this;
    }

    public RawMessageBuilder WithData(string data)
    {
        return WithData(Encoding.UTF8.GetBytes(data));
    }

    public RawMessage Build()
    {
        return new RawMessage(
            key: null,
            headers: new Dictionary<string, string>(),
            data: _data
        );
    }

    public static implicit operator RawMessage(RawMessageBuilder builder)
    {
        return builder.Build();
    }
}