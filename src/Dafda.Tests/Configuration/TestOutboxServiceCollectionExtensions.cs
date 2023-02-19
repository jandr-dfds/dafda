using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Tests.Configuration
{
    public class TestOutboxServiceCollectionExtensions
    {
        [Fact]
        public async Task Can_persist_outbox_message()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();
            var messageId = Guid.NewGuid();

            services.AddOutbox(options =>
            {
                options.WithMessageIdGenerator(new MessageIdGeneratorStub(() => messageId.ToString()));
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithPayloadSerializer("foo", new PayloadSerializerStub("dummy"));

                options.WithOutboxEntryRepository(serviceProvider => fake);
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var entry = fake.OutboxEntries.Single();

            Assert.Equal("foo", entry.Topic);
            Assert.Equal(messageId, entry.MessageId);
            Assert.Equal("baz", entry.Key);
            Assert.Equal("dummy", entry.Payload);
            //Assert.Equal(DateTime.Now, entry.OccurredOnUtc);  // TODO -- should probably be testable
            Assert.Null(entry.ProcessedUtc);
        }

        [Fact]
        public async Task Can_persist_outbox_message_with_special_serializer_format()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();

            services.AddOutbox(options =>
            {
                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithOutboxEntryRepository(serviceProvider => fake);
                options.WithPayloadSerializer("foo", new PayloadSerializerStub("dummy", "expected payload format"));
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});

            var entry = fake.OutboxEntries.Single();

            Assert.Equal("dummy", entry.Payload);
        }

        [Fact]
        public async Task Can_persist_outbox_messages_with_different_serializer_formats()
        {
            var services = new ServiceCollection();
            var fake = new FakeOutboxPersistence();

            services.AddOutbox(options =>
            {
                options.WithOutboxEntryRepository(serviceProvider => fake);

                options.Register<DummyMessage>("foo", "bar", x => "baz");
                options.WithPayloadSerializer("foo", new PayloadSerializerStub("foo_dummy", "expected foo payload format"));

                options.Register<AnotherDummyMessage>("bar", "bar", x => "baz");
                options.WithPayloadSerializer("bar", new PayloadSerializerStub("bar_dummy", "expected foo payload format"));
            });

            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            await outbox.Enqueue(new[] {new DummyMessage()});
            await outbox.Enqueue(new[] {new AnotherDummyMessage()});

            Assert.Equal(
                expected: new[]
                {
                    "foo_dummy",
                    "bar_dummy",
                },
                actual: fake.OutboxEntries.Select(x => x.Payload)
            );
        }

        [Fact]
        public async Task Can_configure_outbox_notifier()
        {
            var services = new ServiceCollection();
            var dummyOutboxNotifier = new DummyNotifier();

            services.AddOutbox(options =>
            {
                options.WithNotifier(dummyOutboxNotifier);
                options.Register<DummyMessage>("foo", "bar", x => "baz");

                options.WithOutboxEntryRepository(serviceProvider => new FakeOutboxPersistence());
            });
            var provider = services.BuildServiceProvider();
            var outbox = provider.GetRequiredService<OutboxQueue>();

            var outboxNotifier = await outbox.Enqueue(new[] {new DummyMessage()});

            Assert.Same(dummyOutboxNotifier, outboxNotifier);
        }


        [Fact]
        public void Must_configure_listener()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidConfigurationException>(() =>
            {
                services.AddOutboxProducer(options =>
                {
                    // NOTE: add minimal configuration for producer; otherwise we see the same exception, but for different reasons
                    // (maybe we need another exception here)
                    options.WithBootstrapServers("localhost");
                });
            });
        }

        public class DummyMessage
        {
        }

        public class AnotherDummyMessage
        {
        }

        private class DummyNotification : IOutboxListener
        {
            public Task<bool> Wait(CancellationToken cancellationToken)
            {
                return Task.FromResult(false);
            }
        }

        private class DummyNotifier : IOutboxNotifier
        {
            public Task Notify(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        public class OutboxListenerSpy : IOutboxListener
        {
            public Task<bool> Wait(CancellationToken cancellationToken)
            {
                Waited = true;
                return Task.FromResult(true);
            }

            public bool Waited { get; private set; }
        }
    }
}
