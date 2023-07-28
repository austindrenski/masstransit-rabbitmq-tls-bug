var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MassTransitHostOptions>(static options => options.StartTimeout = TimeSpan.FromSeconds(10));

builder.Services.Configure<RabbitMqTransportOptions>(static options =>
{
    options.Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
    options.Pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");
    options.Port = 5671;
    options.User = Environment.GetEnvironmentVariable("RABBITMQ_USER");
    options.UseSsl = true;
    options.VHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST");
});

#if SET_EXPLICIT_SSL_PROTOCOLS
builder.Services.AddMassTransit(static configurator => configurator.UsingRabbitMq(static (context, configurator) =>
{
    var options = context.GetRequiredService<IOptions<RabbitMqTransportOptions>>().Value;

    configurator.Host(options.Host, options.Port, options.VHost, rabbitMqHostConfigurator =>
    {
        rabbitMqHostConfigurator.Username(options.User);
        rabbitMqHostConfigurator.Password(options.Pass);
        rabbitMqHostConfigurator.UseSsl(static ssl => ssl.Protocol = SslProtocols.Tls12);
    });
}));
#else
builder.Services.AddMassTransit(static configurator => configurator.UsingRabbitMq());
#endif

using var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var bus = app.Services.GetRequiredService<IBus>();

await bus.Publish(new SomeNamespace.SomeMessage()).ConfigureAwait(false);

namespace SomeNamespace
{
    sealed record SomeMessage;
}
