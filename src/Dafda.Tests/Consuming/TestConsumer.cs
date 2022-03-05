using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Middleware;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                .Register(_ => new MessageHandlerMiddleware(registry, type => handlerStub))
                .Register(_ => new InvocationMiddleware())
                ;

            var serviceProvider = services.BuildServiceProvider();
            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
            var sut = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .WithMiddleware(middlewareBuilder)
                .Build();

            await sut.Consume(consumerScope.Token);

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
                .Register(_ => new MessageHandlerMiddleware(registry, type => null))
                .Register(_ => new InvocationMiddleware())
                ;

            var serviceProvider = services.BuildServiceProvider();
            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
            var sut = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .WithMiddleware(middlewareBuilder)
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(
                () => sut.Consume(consumerScope.Token));
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

            var consumerScope = new CancellingConsumerScope(resultSpy);
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .WithEnableAutoCommit(true)
                .Build();

            await consumer.Consume(consumerScope.Token);

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

            var consumerScope = new CancellingConsumerScope(resultSpy);
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.Consume(consumerScope.Token);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task creates_consumer_scope()
        {
            var messageResultStub = new MessageResultBuilder().WithRawMessage(new RawMessageBuilder()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var consumerScope = new CancellingConsumerScope(messageResultStub);
            var spy = new ConsumerScopeFactorySpy(consumerScope);

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(spy)
                .Build();

            await consumer.Consume(consumerScope.Token);

            Assert.Equal(1, spy.CreateConsumerScopeCalled);
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

            var spy = new CancellingConsumerScope(messageResultStub);

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(spy))
                .Build();

            await consumer.Consume(spy.Token);

            Assert.Equal(1, spy.Disposed);
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