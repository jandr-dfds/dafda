﻿using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when no implementation of <see cref="IMessageHandler{T}"/> was supplied
    /// to the <see cref="IHandlerUnitOfWork.Run"/> method.
    /// </summary>
    [Obsolete("This will be removed in a future release")]
    public sealed class InvalidMessageHandlerException : Exception
    {
        internal InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}