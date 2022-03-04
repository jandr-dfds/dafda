using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
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
            var handlerMock = new Mock<IMessageHandler<FooMessage>>();
            var handlerStub = handlerMock.Object;

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
            var sut = new ConsumerBuilder()
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .Build();

            await sut.Consume(consumerScope.Token);

            handlerMock.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<MessageHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task throws_when_consuming_an_unknown_message_when_explicit_handlers_are_required()
        {
            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
            var sut = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(
                () => sut.Consume(consumerScope.Token));
        }

        [Fact]
        public async Task does_not_throw_when_consuming_an_unknown_message_with_no_op_strategy()
        {
            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder().Build());
            var sut =
                new ConsumerBuilder()
                    .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                    .WithUnitOfWork(
                        new UnitOfWorkStub(
                            new NoOpHandler(new Mock<ILogger<NoOpHandler>>().Object)))
                    .WithUnconfiguredMessageStrategy(new UseNoOpHandler())
                    .Build();

            await sut.Consume(consumerScope.Token);
        }

        [Fact]
        public async Task expected_order_of_handler_invocation_in_unit_of_work()
        {
            var orderOfInvocation = new LinkedList<string>();

            var dummyMessageResult = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var dummyMessageRegistration = new MessageRegistrationBuilder().WithMessageType("foo").Build();

            var registry = new MessageHandlerRegistry();
            registry.Register(dummyMessageRegistration);

            var consumerScope = new CancellingConsumerScope(dummyMessageResult);
            var sut = new ConsumerBuilder()
                .WithUnitOfWork(new UnitOfWorkSpy(
                    handlerInstance: new MessageHandlerSpy<FooMessage>(() => orderOfInvocation.AddLast("during")),
                    pre: () => orderOfInvocation.AddLast("before"),
                    post: () => orderOfInvocation.AddLast("after")
                ))
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(consumerScope))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await sut.Consume(consumerScope.Token);

            Assert.Equal(new[] {"before", "during", "after"}, orderOfInvocation);
        }

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
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
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
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.Consume(consumerScope.Token);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task creates_consumer_scope()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
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
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await consumer.Consume(consumerScope.Token);

            Assert.Equal(1, spy.CreateConsumerScopeCalled);
        }

        [Fact]
        public async Task disposes_consumer_scope()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
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
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await consumer.Consume(spy.Token);

            Assert.Equal(1, spy.Disposed);
        }

        #region helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }
}
