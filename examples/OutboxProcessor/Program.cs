using System;
using Dafda.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutboxProcessor;
using Serilog;
using Serilog.Events;

const string applicationName = "Sample.Outbox.Processor";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Dafda", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: $"{applicationName.ToUpper()} [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
    .CreateLogger();

try
{
    Log.Information($"Starting {applicationName} application");

    await Host.CreateDefaultBuilder(args)
        .UseSerilog(Log.Logger)
        .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables(); })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            // configure persistence (Postgres)
            var connectionString = configuration["SAMPLE_OUTBOX_PROCESSOR_CONNECTION_STRING"];
            var channel = configuration["SAMPLE_OUTBOX_PROCESSOR_CHANNEL_NAME"];

            var outboxNotification = new PostgresListener(connectionString, channel, TimeSpan.FromSeconds(30));

            services.AddSingleton(provider => outboxNotification); // register to dispose

            services.AddOutboxProducer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");

                // include outbox (polling publisher)
                options.WithUnitOfWorkFactory(_ => new OutboxUnitOfWorkFactory(connectionString));
                options.WithListener(outboxNotification);
            });
        })
        .Build()
        .RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, $"{applicationName} application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static IHostBuilder CreateHostBuilder()
{
    return new HostBuilder()
        .UseSerilog()
        .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables(); })
        .ConfigureServices((hostContext, services) =>
        {
            var configuration = hostContext.Configuration;

            // configure persistence (Postgres)
            var connectionString = configuration["SAMPLE_OUTBOX_PROCESSOR_CONNECTION_STRING"];
            var channel = configuration["SAMPLE_OUTBOX_PROCESSOR_CHANNEL_NAME"];

            var outboxNotification = new PostgresListener(connectionString, channel, TimeSpan.FromSeconds(30));

            services.AddSingleton(provider => outboxNotification); // register to dispose

            services.AddOutboxProducer(options =>
            {
                // configuration settings
                options.WithConfigurationSource(configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA");

                // include outbox (polling publisher)
                options.WithUnitOfWorkFactory(_ => new OutboxUnitOfWorkFactory(connectionString));
                options.WithListener(outboxNotification);
            });
        });
}