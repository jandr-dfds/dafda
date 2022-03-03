using System;

namespace Dafda.Consuming;

/// <summary>
/// The incoming message after it has been deserialized.
/// </summary>
public sealed class IncomingMessage
{
    internal IncomingMessage(Type messageType, Metadata metadata, object instance)
    {
        MessageType = messageType;
        Metadata = metadata;
        Instance = instance;
    }

    /// <summary>
    /// The <see cref="Type"/> of the message.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// The message <see cref="Metadata"/>.
    /// </summary>
    public Metadata Metadata { get; }

    /// <summary>
    /// The message instance.
    /// </summary>
    public object Instance { get; }
}