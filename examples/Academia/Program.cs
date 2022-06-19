#define NOTIFY_INPROGRESS
#define NOTIFY_POSTGRES

using System;
using Academia;
using Academia.Application;
using Academia.Controllers;
using Academia.Domain;
using Academia.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

const string ApplicationName = "InProcessOutbox";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Dafda", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: $"{ApplicationName.ToUpper()} [{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{Message:lj}}{{NewLine}}{{Exception}}")
    .CreateLogger();

try
{
    Log.Information($"Starting {ApplicationName} application");

    var builder = WebApplication.CreateBuilder();
    builder.Host.UseSerilog(Log.Logger);

    builder.Services.AddSingleton<Stats>();
    builder.Services.AddTransient<StudentApplicationService>();

    // domain events are scoped so we can access them in TransactionalOutbox
    builder.Services.AddScoped<DomainEvents>();
    builder.Services.AddScoped<IDomainEvents>(provider => provider.GetRequiredService<DomainEvents>());

    // configure persistence (Postgres)
    var connectionString = builder.Configuration["ACADEMIA_CONNECTION_STRING"];
    builder.Services.AddDbContext<SampleDbContext>(options => options.UseNpgsql(connectionString));
    builder.Services.AddTransient<IStudentRepository, RelationalStudentRepository>();

    // configure messaging
#if NOTIFY_INPROGRESS
    builder.Services.AddInProcessOutboxMessaging(builder.Configuration);
#elif NOTIFY_POSTGRES
    builder.Services.AddPostgresNotifyOutboxMessaging(builder.Configuration);
#else
    builder.Services.AddOutOfBandOutboxMessaging(builder.Configuration);
#endif

    // configure web api
    var app = builder.Build();
    app.UseTransactionalMiddleware();
    app.MapStudentModule();
    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, $"{ApplicationName} application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

