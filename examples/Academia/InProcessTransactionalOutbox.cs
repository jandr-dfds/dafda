using System;
using System.Threading;
using System.Threading.Tasks;
using Academia.Application;
using Academia.Domain;
using Academia.Infrastructure.Persistence;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Academia
{
    public static class InProcessOutboxMessaging
    {
        private const string StudentsTopic = "academia.students";

        public static void AddInProcessOutboxMessaging(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<ITransactionalOutbox, TransactionalOutbox>();

            // configure messaging: consumer
            builder.Services.AddConsumer(options =>
            {
                // kafka consumer settings
                options.WithConfigurationSource(builder.Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA", "ACADEMIA_KAFKA");

                // register message handlers
                options.RegisterMessageHandler<StudentEnrolled, StudentEnrolledHandler>(StudentsTopic, StudentEnrolled.MessageType);
                options.RegisterMessageHandler<StudentChangedEmail, StudentChangedEmailHandler>(StudentsTopic, StudentChangedEmail.MessageType);
            });

            // register the outbox in-process notification mechanism
            var outboxNotification = new OutboxNotification(TimeSpan.FromSeconds(5));
            builder.Services.AddSingleton(_ => outboxNotification); // register to dispose

            // configure the outbox pattern using Dafda
            builder.Services.AddOutbox(options =>
            {
                // register outgoing (through the outbox) messages
                options.Register<StudentEnrolled>(StudentsTopic, StudentEnrolled.MessageType, @event => @event.StudentId);
                options.Register<StudentChangedEmail>(StudentsTopic, StudentChangedEmail.MessageType, @event => @event.StudentId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();

                // add notifier (for immediate dispatch)
                options.WithNotifier(outboxNotification);
            });

            // configure the outbox producer
            builder.Services.AddOutboxProducer(options =>
            {
                // kafka producer settings
                options.WithConfigurationSource(builder.Configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA", "ACADEMIA_KAFKA");

                // include outbox unit of work (so we can read/update the outbox table)
                options.WithUnitOfWorkFactory<OutboxUnitOfWorkFactory>();

                // add listener (for immediate dispatch)
                options.WithListener(outboxNotification);
            });
        }

        private class TransactionalOutbox : ITransactionalOutbox
        {
            private readonly SampleDbContext _dbContext;
            private readonly OutboxQueue _outboxQueue;
            private readonly DomainEvents _domainEvents;

            public TransactionalOutbox(SampleDbContext dbContext, OutboxQueue outboxQueue, DomainEvents domainEvents)
            {
                _dbContext = dbContext;
                _outboxQueue = outboxQueue;
                _domainEvents = domainEvents;
            }

            public Task Execute(Func<Task> action)
            {
                return Execute(action, CancellationToken.None);
            }

            public async Task Execute(Func<Task> action, CancellationToken cancellationToken)
            {
                IOutboxNotifier outboxNotifier;

                await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    await action();

                    outboxNotifier = await _outboxQueue.Enqueue(_domainEvents.Events);

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }

                if (outboxNotifier != null)
                {
                    // NOTE: when using postgres LISTEN/NOTIFY this should/could be part of the transaction scope above
                    await outboxNotifier.Notify(cancellationToken);
                }
            }
        }
    }
}