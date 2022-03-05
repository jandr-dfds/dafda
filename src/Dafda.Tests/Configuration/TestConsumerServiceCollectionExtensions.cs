using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerServiceCollectionExtensions
    {
        [Fact( /*Skip = "is this relevant for testing these extensions"*/)]
        public async Task Can_consume_messages()
        {
            var loops = 0;
            var consumerScope = new CancellingConsumerScope(new MessageResultBuilder()
                .WithRawMessage(new RawMessageBuilder())
                .Build(), 2);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeDecoratorWithHooks(
                    inner: consumerScope,
                    postHook: () => { loops++; }
                )))
                .Build();

            using var consumerHostedService = new ConsumerHostedService(
                NullLogger<ConsumerHostedService>.Instance,
                new DummyApplicationLifetime(),
                consumer,
                "dummy.group.id",
                ConsumerErrorHandler.Default
            );

            await consumerHostedService.ConsumeAll(consumerScope.Token);

            Assert.Equal(2, loops);
        }

        [Fact]
        public void add_single_consumer_registers_a_single_hosted_service()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
            });

            var serviceProvider = services.BuildServiceProvider();
            var consumerHostedServices = serviceProvider
                .GetServices<IHostedService>()
                .OfType<ConsumerHostedService>();

            Assert.Single(consumerHostedServices);
        }

        [Fact]
        public void add_multiple_consumers_registers_multiple_hosted_services()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime, DummyApplicationLifetime>();

            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId 1");
            });

            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId 2");
            });

            var serviceProvider = services.BuildServiceProvider();
            var consumerHostedServices = serviceProvider
                .GetServices<IHostedService>()
                .OfType<ConsumerHostedService>();

            Assert.Equal(2, consumerHostedServices.Count());
        }

        [Fact]
        public void throws_exception_when_registering_multiple_consumers_with_same_consumer_group_id()
        {
            var consumerGroupId = "foo";

            var services = new ServiceCollection();
            services.AddConsumer(options =>
            {
                options.WithGroupId(consumerGroupId);
                options.WithBootstrapServers("dummy");
            });

            Assert.Throws<InvalidConfigurationException>(() =>
            {
                services.AddConsumer(options =>
                {
                    options.WithBootstrapServers("dummy");
                    options.WithGroupId(consumerGroupId);
                });
            });
        }

        [Fact]
        public async Task default_consumer_failure_strategy_will_stop_application()
        {
            var spy = new ApplicationLifetimeSpy();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime>(_ => spy);
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.WithConsumerScopeFactory(_ => new FailingConsumerScopeFactory());
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumerHostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<ConsumerHostedService>()
                .Single();

            await consumerHostedService.ConsumeLoop(CancellationToken.None);

            Assert.True(spy.StopApplicationWasCalled);
        }

        [Fact]
        public async Task consumer_failure_strategy_is_evaluated()
        {
            const int failuresBeforeQuitting = 2;
            var count = 0;

            var spy = new ApplicationLifetimeSpy();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHostApplicationLifetime>(_ => spy);
            services.AddConsumer(options =>
            {
                options.WithBootstrapServers("dummyBootstrapServer");
                options.WithGroupId("dummyGroupId");
                options.WithConsumerScopeFactory(_ => new FailingConsumerScopeFactory());
                options.WithConsumerErrorHandler(exception =>
                {
                    if (++count > failuresBeforeQuitting)
                    {
                        return Task.FromResult(ConsumerFailureStrategy.Default);
                    }

                    return Task.FromResult(ConsumerFailureStrategy.RestartConsumer);
                });
            });
            var serviceProvider = services.BuildServiceProvider();

            var consumerHostedService = serviceProvider.GetServices<IHostedService>()
                .OfType<ConsumerHostedService>()
                .Single();

            await consumerHostedService.ConsumeLoop(CancellationToken.None);

            Assert.Equal(failuresBeforeQuitting + 1, count);
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public static object LastHandledMessage { get; private set; }
        }

        private class ConsumerScopeDecoratorWithHooks : ConsumerScope
        {
            private readonly ConsumerScope _inner;
            private readonly Action _preHook;
            private readonly Action _postHook;

            public ConsumerScopeDecoratorWithHooks(ConsumerScope inner, Action preHook = null, Action postHook = null)
            {
                _inner = inner;
                _preHook = preHook;
                _postHook = postHook;
            }

            public override async Task<MessageResult> GetNext(CancellationToken cancellationToken)
            {
                _preHook?.Invoke();
                var result = await _inner.GetNext(cancellationToken);
                _postHook?.Invoke();

                return result;
            }

            public override void Dispose()
            {
                _inner.Dispose();
            }
        }
        private class FailingConsumerScopeFactory : IConsumerScopeFactory
        {
            public ConsumerScope CreateConsumerScope()
            {
                throw new System.InvalidOperationException();
            }
        }
    }
}