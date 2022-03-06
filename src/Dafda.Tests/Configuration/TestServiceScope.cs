using System;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestServiceScope
    {
        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_transient()
        {
            var createCount = 0;
            var disposeCount = 0;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<Repository>();
            services.AddTransient(_ => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var options = new ConsumerOptions(services);
            options.WithGroupId("dummy");
            options.WithBootstrapServers("dummy");
            options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));
            options.WithDeserializer(DeserializerStub.Returns(new DummyMessage()));
            var consumerConfiguration = options.Build();

            var serviceProvider = services.BuildServiceProvider();

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(new MessageResultBuilder().Build()))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .WithMiddleware(consumerConfiguration.MiddlewareBuilder)
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .Build();

            await consumer.Consume(Consume.Twice);

            Assert.Equal(4, createCount);
            Assert.Equal(4, disposeCount);
        }

        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_singleton()
        {
            var createCount = 0;
            var disposeCount = 0;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<Repository>();
            services.AddSingleton(_ => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var options = new ConsumerOptions(services);
            options.WithGroupId("dummy");
            options.WithBootstrapServers("dummy");
            options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));
            options.WithDeserializer(DeserializerStub.Returns(new DummyMessage()));
            var consumerConfiguration = options.Build();

            var serviceProvider = services.BuildServiceProvider();

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(new MessageResultBuilder().Build()))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .WithMiddleware(consumerConfiguration.MiddlewareBuilder)
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .Build();

            await consumer.Consume(Consume.Twice);

            Assert.Equal(1, createCount);
            Assert.Equal(0, disposeCount);
        }

        [Fact]
        public async Task Has_expected_number_of_creations_and_disposals_when_scoped()
        {
            var createCount = 0;
            var disposeCount = 0;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddTransient<Repository>();
            services.AddScoped(_ => new ScopeSpy(onCreate: () => createCount++, onDispose: () => disposeCount++));

            var options = new ConsumerOptions(services);
            options.WithGroupId("dummy");
            options.WithBootstrapServers("dummy");
            options.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));
            options.WithDeserializer(DeserializerStub.Returns(new DummyMessage()));
            var consumerConfiguration = options.Build();

            var serviceProvider = services.BuildServiceProvider();

            var consumer = new ConsumerBuilder()
                .WithConsumerScope(new ConsumerScopeStub(new MessageResultBuilder().Build()))
                .WithEnableAutoCommit(consumerConfiguration.EnableAutoCommit)
                .WithMiddleware(consumerConfiguration.MiddlewareBuilder)
                .WithServiceScopeFactory(serviceProvider.GetRequiredService<IServiceScopeFactory>())
                .Build();

            await consumer.Consume(Consume.Twice);

            Assert.Equal(2, createCount);
            Assert.Equal(2, disposeCount);
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            private readonly ScopeSpy _scopeSpy;
            private readonly Repository _repository;

            public DummyMessageHandler(ScopeSpy scopeSpy, Repository repository)
            {
                _scopeSpy = scopeSpy;
                _repository = repository;
            }

            public async Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                await _scopeSpy.DoSomethingAsync();
                await _repository.PerformActionAsync();
            }
        }

        public class Repository
        {
            private readonly ScopeSpy _scopeSpy;

            public Repository(ScopeSpy scopeSpy)
            {
                _scopeSpy = scopeSpy;
            }

            public async Task PerformActionAsync()
            {
                await _scopeSpy.DoSomethingAsync();
            }
        }

        public class ScopeSpy : IDisposable
        {
            private readonly Action _onCreate;
            private readonly Action _onDispose;

            private bool _diposed;

            public ScopeSpy(Action onCreate, Action onDispose)
            {
                _onCreate = onCreate;
                _onDispose = onDispose;

                _onCreate?.Invoke();
            }

            public void Dispose()
            {
                _diposed = true;
                _onDispose?.Invoke();
            }

            public async Task DoSomethingAsync()
            {
                if (_diposed)
                {
                    throw new ObjectDisposedException(nameof(ScopeSpy), "Ups, already disposed!");
                }

                await Task.Delay(10);
            }
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
    }
}