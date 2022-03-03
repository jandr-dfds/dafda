using System;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    internal class MessageRegistrationBuilder
    {
        private string _topic;
        private string _messageType;
        private Type _handlerInstanceType;
        private Type _messageInstanceType;
        private MessageHandlerDelegate _messageHandler;

        public MessageRegistrationBuilder()
        {
            _topic = "dummy topic";
            _messageType = "dummy message type";
            _handlerInstanceType = typeof(FooHandler);
            _messageInstanceType = typeof(FooMessage);
            _messageHandler = MessageHandlerDelegate.Create<FooMessage, FooHandler>();
        }

        public MessageRegistrationBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public MessageRegistrationBuilder WithMessageType(string messageType)
        {
            _messageType = messageType;
            return this;
        }

        public MessageRegistrationBuilder WithHandlerInstanceType(Type handlerInstanceType)
        {
            _handlerInstanceType = handlerInstanceType;
            return this;
        }

        public MessageRegistrationBuilder WithMessageInstanceType(Type messageInstanceType)
        {
            _messageInstanceType = messageInstanceType;
            return this;
        }


        public MessageRegistrationBuilder WithMessageHandler(MessageHandlerDelegate messageHandler)
        {
            _messageHandler = messageHandler;
            return this;
        }
        
        public MessageRegistration Build()
        {
            return new MessageRegistration(
                handlerInstanceType: _handlerInstanceType,
                messageInstanceType: _messageInstanceType,
                topic: _topic,
                messageType: _messageType,
                messageHandler: _messageHandler);
        }

        #region private helper classes

        private class FooMessage
        {

        }

        private class FooHandler : IMessageHandler<FooMessage>
        {
            public Task Handle(FooMessage message, MessageHandlerContext context)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}