using System;
using System.Threading;
using System.Threading.Tasks;
using Academia.Application;
using Academia.Domain;
using Academia.Infrastructure.Persistence;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Academia
{
    public static class PostgresNotifyOutboxMessaging
    {
        private const string StudentsTopic = "academia.students";
        private const string DafdaOutboxPostgresChannel = "dafda";

        public static void AddPostgresNotifyOutboxMessaging(this WebApplicationBuilder builder)
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

            // configure the outbox pattern using Dafda
            builder.Services.AddOutbox(options =>
            {
                // register outgoing (through the outbox) messages
                options.Register<StudentEnrolled>(StudentsTopic, StudentEnrolled.MessageType, @event => @event.StudentId);
                options.Register<StudentChangedEmail>(StudentsTopic, StudentChangedEmail.MessageType, @event => @event.StudentId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();

                // no notifier configured
            });

            // the outbox producer is configured out-of-band
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
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                await action();

                await _outboxQueue.Enqueue(_domainEvents.Events);

                // NOTE: we don't use the built-in notification mechanism,
                // instead we rely on postgres' LISTEN/NOTIFY
                await _dbContext.Database.ExecuteSqlRawAsync($"NOTIFY {DafdaOutboxPostgresChannel};", cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        }

    }
}