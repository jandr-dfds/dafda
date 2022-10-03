﻿using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Dafda.Middleware;

namespace Dafda.Producing
{
    /// <summary>
    /// Produce messages on Kafka
    /// </summary>
    public sealed class Producer
    {
        private readonly Pipeline _pipeline;

        internal Producer(Pipeline pipeline)
        {
            _pipeline = pipeline;
        }

        internal string Name { get; init; } = "__Default Producer__";
        
        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        public async Task Produce(object message)
        {
            await Produce(message, new Metadata());
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="headers">The message headers</param>
        public async Task Produce(object message, Metadata headers)
        {
            await _pipeline.Invoke(new OutgoingMessageContext(new OutgoingMessage(message, headers), new RootMiddlewareContext(null)));
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka including <paramref name="headers"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="headers">The message headers</param>
        public async Task Produce(object message, Dictionary<string, object> headers)
        {
            var dict = headers.ToDictionary( pair => pair.Key, pair => pair.Value.ToString());
            await Produce(message, new Metadata( dict ));
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new message</param>
        public async Task Produce(object message, MessageHandlerContext context)
        {
            await Produce(message, context, new Dictionary<string, string>());
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new message</param>
        /// <param name="headers">Additional message headers</param>
        public async Task Produce(object message, MessageHandlerContext context, Dictionary<string, string> headers)
        {
            var metadata = new Metadata(headers)
            {
                CorrelationId = context.CorrelationId,
                CausationId = context.MessageId
            };
            
            await Produce(message, metadata);
        }
    }
}