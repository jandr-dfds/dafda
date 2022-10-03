using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Consuming
{
    public class TestConsumer
    {
        [Fact]
        public async Task invokes_expected_handler_when_consuming()
        {
            var wasCalled = false;

            var handlerStub = new MessageHandlerSpy<FooMessage>(() => { wasCalled = true; });

            var registry = new MessageHandlerRegistry();
            registry.Register<FooMessage, MessageHandlerSpy<FooMessage>>("", "");

            var services = new ServiceCollection();
            var middlewareBuilder = new MiddlewareBuilder<IncomingRawMessageContext>(services);
            middlewareBuilder
                .Register(_ => new DeserializationMiddleware(DeserializerStub.Returns(new FooMessage())))
                .Register(_ => new MessageHandlerMiddleware(registry, _ => handlerStub))
                .Register(_ => new InvocationMiddleware())
                ;

            var serviceProvider = services.BuildServiceProvider();
            var sut = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(new MessageResultBuilder().Build()))
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .WithMiddleware(middlewareBuilder)
                .Build();

            await sut.Consume(Consume.Once);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task throws_when_consuming_an_unknown_message_when_explicit_handlers_are_required()
        {
            var registry = new MessageHandlerRegistry();

            var services = new ServiceCollection();
            var middlewareBuilder = new MiddlewareBuilder<IncomingRawMessageContext>(services);
            middlewareBuilder
                .Register(_ => new DeserializationMiddleware(DeserializerStub.Returns(new FooMessage())))
                .Register(_ => new MessageHandlerMiddleware(registry, _ => null))
                .Register(_ => new InvocationMiddleware())
                ;

            var serviceProvider = services.BuildServiceProvider();
            var sut = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(new MessageResultBuilder().Build()))
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .WithMiddleware(middlewareBuilder)
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(
                () => sut.Consume(Consume.Once));
        }

        // [Fact]
        // public async Task does_not_throw_when_consuming_an_unknown_message_with_no_op_strategy()
        // {
        //     var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
        //     var sut =
        //         new ConsumerBuilder()
        //             .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
        //             .WithUnconfiguredMessageStrategy(new UseNoOpHandler())
        //             .Build();
        //
        //     await sut.Consume(consumerScope.Token);
        // }

        [Fact]
        public async Task will_not_call_commit_when_auto_commit_is_enabled()
        {
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var wasCalled = false;

            var resultSpy = new MessageResultBuilder()
                .WithOnCommit(() =>
                {
                    wasCalled = true;
                    return Task.CompletedTask;
                })
                .Build();

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(resultSpy))
                .WithEnableAutoCommit(true)
                .Build();

            await consumer.Consume(Consume.Once);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task will_call_commit_when_auto_commit_is_disabled()
        {
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var wasCalled = false;

            var resultSpy = new MessageResultBuilder()
                .WithOnCommit(() =>
                {
                    wasCalled = true;
                    return Task.CompletedTask;
                })
                .Build();

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(resultSpy))
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.Consume(Consume.Once);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task disposes_consumer_scope()
        {
            var messageResultStub = new MessageResultBuilder().WithRawMessage(new RawMessageBuilder().Build()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var spy = new ConsumerScopeSpy(messageResultStub);

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(spy)
                .Build();

            await consumer.Consume(Consume.Once);

            Assert.Equal(1, spy.ConsumeCalled);
            Assert.True(spy.Disposed);
        }

        #region helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        private class DeserializerStub : IDeserializer
        {
            public static IDeserializer Returns<T>(T instance)
            {
                return new DeserializerStub(typeof(T), instance);
            }

            private readonly Type _messageType;
            private readonly object _instance;

            public DeserializerStub(Type messageType, object instance)
            {
                _messageType = messageType;
                _instance = instance;
            }

            public IncomingMessage Deserialize(RawMessage message)
            {
                return new IncomingMessage(_messageType, new Metadata(), _instance);
            }
        }

        #endregion
    }
}