using System.Linq;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerServiceCollectionExtensions
    {
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
    }
}