﻿using System;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    [Obsolete("This will be removed in a future release")]
    internal class PoisonAwareIncomingMessageFactory : IIncomingMessageFactory
    {
        private readonly ILogger<PoisonAwareIncomingMessageFactory> _logger;
        private readonly IIncomingMessageFactory _innerIncomingMessageFactory;

        public PoisonAwareIncomingMessageFactory(ILogger<PoisonAwareIncomingMessageFactory> logger, IIncomingMessageFactory innerIncomingMessageFactory)
        {
            _logger = logger;
            _innerIncomingMessageFactory = innerIncomingMessageFactory;
        }

        public TransportLevelMessage Create(string rawMessage)
        {
            try
            {
                return _innerIncomingMessageFactory.Create(rawMessage);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Exception thrown when creating transport level message");
                return new TransportLevelMessage(new Metadata() { Type = TransportLevelPoisonMessage.Type }, _ => new TransportLevelPoisonMessage(rawMessage, ex));
            }
        }
    }
}
