﻿using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    internal class MessageResultBuilder
    {
        private RawMessage _message = new RawMessageBuilder().Build();
        private Func<Task> _onCommit;

        public MessageResultBuilder()
        {
            _onCommit = () => Task.CompletedTask;
        }

        public MessageResultBuilder WithRawMessage(RawMessage message)
        {
            _message = message;
            return this;
        }

        public MessageResultBuilder WithOnCommit(Func<Task> onCommit)
        {
            _onCommit = onCommit;
            return this;
        }

        public MessageResult Build()
        {
            return new MessageResult(_message, _onCommit);
        }
    }
}