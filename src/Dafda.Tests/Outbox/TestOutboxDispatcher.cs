using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxDispatcher
    {
        [Fact]
        public async Task Can_processes_unpublished_outbox_messages()
        {
            var spy = new KafkaProducerSpy();
            var sut = A.OutboxDispatcher
                .With(new FakeOutboxPersistence(A.OutboxEntry
                    .WithTopic("foo")
                    .WithKey("bar")
                    .WithValue("baz")
                ))
                .With(A.OutboxProducer
                    .With(spy)
                    .Build()
                )
                .Build();

            await sut.Dispatch(CancellationToken.None);

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("bar", spy.Key);
            Assert.Equal("baz", spy.Value);
        }

        private static class A
        {
            public static OutboxProducerBuilder OutboxProducer => new();
            public static OutboxDispatcherBuilder OutboxDispatcher => new();
            public static OutboxEntryBuilder OutboxEntry => new();
        }
    }
}