# Getting Started

See: MassTransit/MassTransit#4522

## Background

This repository demonstrates a (potential) MassTransit bug caused by the default [`ConfigurationHostSettings.SslProtocol`][ConfigurationHostSettings]
and some inconsistency between the [constructor overloads of `RabbitMqHostConfigurator`][RabbitMqHostConfigurator].

Note the default for `ConfigurationHostSettings.SslProtocol` was recently updated from `SslProtocols.Tls` to `SslProtocols.Tls12` by
MassTransit/MassTransit#4449, but wanted to report this specifically since the inconsistent behavior remains:

```csharp
public ConfigurationHostSettings()
{
    var defaultOptions = new SslOption();
    SslProtocol = SslProtocols.Tls;
```

```csharp
public RabbitMqHostConfigurator(Uri hostAddress, string connectionName = null)
{
    _settings = hostAddress.GetConfigurationHostSettings();


    if (_settings.Port == 5671)
    {
        UseSsl(s =>
        {
            s.Protocol = SslProtocols.Tls12;
        });
    }


    _settings.VirtualHost = Uri.UnescapeDataString(GetVirtualHost(hostAddress));


    if (!string.IsNullOrEmpty(connectionName))
        _settings.ClientProvidedName = connectionName;
}
```

```csharp
public RabbitMqHostConfigurator(string host, string virtualHost, ushort port = 5672, string connectionName = null)
{
    _settings = new ConfigurationHostSettings
    {
        Host = host,
        Port = port,
        VirtualHost = virtualHost
    };


    if (!string.IsNullOrEmpty(connectionName))
        _settings.ClientProvidedName = connectionName;
}
```

## Example

For a self-contained reproduction,
see [the GitHub Actions in this repository](https://github.com/austindrenski/masstransit-rabbitmq-tls-bug/actions/ci.yml).

```console
$ dotnet run -p:DefineConstants=SET_EXPLICIT_SSL_PROTOCOLS

info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /home/runner/work/masstransit-rabbitmq-tls-bug/masstransit-rabbitmq-tls-bug
info: MassTransit[0]
      Bus started: rabbitmqs://***/***
info: MassTransit[0]
      Bus stopped: rabbitmqs://***/***
```

```console
$ dotnet run

info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /home/runner/work/masstransit-rabbitmq-tls-bug/masstransit-rabbitmq-tls-bug
warn: MassTransit[0]
      Connection Failed: rabbitmqs://***/***
      RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
       ---> System.AggregateException: One or more errors occurred. (Authentication failed, see inner exception.)
       ---> System.Security.Authentication.AuthenticationException: Authentication failed, see inner exception.
       ---> Interop+OpenSsl+SslException: SSL Handshake failed with OpenSSL error - SSL_ERROR_SSL.
       ---> Interop+Crypto+OpenSslCryptographicException: error:0A0000BF:SSL routines::no protocols available
         --- End of inner exception stack trace ---
         at Interop.OpenSsl.DoSslHandshake(SafeSslHandle context, ReadOnlySpan`1 input, Byte[]& sendBuf, Int32& sendCount)
         at System.Net.Security.SslStreamPal.HandshakeInternal(SafeDeleteSslContext& context, ReadOnlySpan`1 inputBuffer, Byte[]& outputBuffer, SslAuthenticationOptions sslAuthenticationOptions, SelectClientCertificate clientCertificateSelectionCallback)
         --- End of inner exception stack trace ---
         at System.Net.Security.SslStream.ForceAuthenticationAsync[TIOAdapter](Boolean receiveFirst, Byte[] reAuthenticationData, CancellationToken cancellationToken)
         at RabbitMQ.Client.Impl.SslHelper.<>c__DisplayClass2_0.<TcpUpgrade>b__0(SslOption opts)
         at RabbitMQ.Client.Impl.SslHelper.TcpUpgrade(Stream tcpStream, SslOption options)
         at RabbitMQ.Client.Impl.SocketFrameHandler..ctor(AmqpTcpEndpoint endpoint, Func`2 socketFactory, TimeSpan connectionTimeout, TimeSpan readTimeout, TimeSpan writeTimeout)
         at RabbitMQ.Client.Framing.Impl.IProtocolExtensions.CreateFrameHandler(IProtocol protocol, AmqpTcpEndpoint endpoint, ArrayPool`1 pool, Func`2 socketFactory, TimeSpan connectionTimeout, TimeSpan readTimeout, TimeSpan writeTimeout)
         at RabbitMQ.Client.ConnectionFactory.CreateFrameHandler(AmqpTcpEndpoint endpoint)
         at RabbitMQ.Client.EndpointResolverExtensions.SelectOne[T](IEndpointResolver resolver, Func`2 selector)
         --- End of inner exception stack trace ---
         at RabbitMQ.Client.EndpointResolverExtensions.SelectOne[T](IEndpointResolver resolver, Func`2 selector)
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IEndpointResolver endpointResolver, String clientProvidedName)
         --- End of inner exception stack trace ---
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IEndpointResolver endpointResolver, String clientProvidedName)
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IList`1 hostnames, String clientProvidedName)
         at MassTransit.RabbitMqTransport.ConnectionContextFactory.CreateConnection(ISupervisor supervisor) in /_/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/ConnectionContextFactory.cs:line 86
warn: MassTransit[0]
      Connection Failed: rabbitmqs://***/***
      RabbitMQ.Client.Exceptions.BrokerUnreachableException: None of the specified endpoints were reachable
       ---> System.AggregateException: One or more errors occurred. (Authentication failed, see inner exception.)
       ---> System.Security.Authentication.AuthenticationException: Authentication failed, see inner exception.
       ---> Interop+OpenSsl+SslException: SSL Handshake failed with OpenSSL error - SSL_ERROR_SSL.
       ---> Interop+Crypto+OpenSslCryptographicException: error:0A0000BF:SSL routines::no protocols available
         --- End of inner exception stack trace ---
         at Interop.OpenSsl.DoSslHandshake(SafeSslHandle context, ReadOnlySpan`1 input, Byte[]& sendBuf, Int32& sendCount)
         at System.Net.Security.SslStreamPal.HandshakeInternal(SafeDeleteSslContext& context, ReadOnlySpan`1 inputBuffer, Byte[]& outputBuffer, SslAuthenticationOptions sslAuthenticationOptions, SelectClientCertificate clientCertificateSelectionCallback)
         --- End of inner exception stack trace ---
         at System.Net.Security.SslStream.ForceAuthenticationAsync[TIOAdapter](Boolean receiveFirst, Byte[] reAuthenticationData, CancellationToken cancellationToken)
         at RabbitMQ.Client.Impl.SslHelper.<>c__DisplayClass2_0.<TcpUpgrade>b__0(SslOption opts)
         at RabbitMQ.Client.Impl.SslHelper.TcpUpgrade(Stream tcpStream, SslOption options)
         at RabbitMQ.Client.Impl.SocketFrameHandler..ctor(AmqpTcpEndpoint endpoint, Func`2 socketFactory, TimeSpan connectionTimeout, TimeSpan readTimeout, TimeSpan writeTimeout)
         at RabbitMQ.Client.Framing.Impl.IProtocolExtensions.CreateFrameHandler(IProtocol protocol, AmqpTcpEndpoint endpoint, ArrayPool`1 pool, Func`2 socketFactory, TimeSpan connectionTimeout, TimeSpan readTimeout, TimeSpan writeTimeout)
         at RabbitMQ.Client.ConnectionFactory.CreateFrameHandler(AmqpTcpEndpoint endpoint)
         at RabbitMQ.Client.EndpointResolverExtensions.SelectOne[T](IEndpointResolver resolver, Func`2 selector)
         --- End of inner exception stack trace ---
         at RabbitMQ.Client.EndpointResolverExtensions.SelectOne[T](IEndpointResolver resolver, Func`2 selector)
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IEndpointResolver endpointResolver, String clientProvidedName)
         --- End of inner exception stack trace ---
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IEndpointResolver endpointResolver, String clientProvidedName)
         at RabbitMQ.Client.ConnectionFactory.CreateConnection(IList`1 hostnames, String clientProvidedName)
         at MassTransit.RabbitMqTransport.ConnectionContextFactory.CreateConnection(ISupervisor supervisor) in /_/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/ConnectionContextFactory.cs:line 86
warn: MassTransit[0]
      Failed to stop bus: rabbitmqs://***/***/fvaz615133_masstransitrabbitmqtlsbug_bus_uydoyyb4gwyy5gi9bdpa9ousbw?temporary=true (Not Started)
info: MassTransit[0]
      Bus stopped: rabbitmqs://***/***
Unhandled exception. System.Threading.Tasks.TaskCanceledException: A task was canceled.
   at MassTransit.Transports.HostConfigurationRetryExtensions.Retry(IHostConfiguration hostConfiguration, Func`1 factory, CancellationToken cancellationToken, CancellationToken stoppingToken) in /_/src/MassTransit/Transports/HostConfigurationRetryExtensions.cs:line 32
   at Program.<Main>$(String[] args) in /home/runner/work/masstransit-rabbitmq-tls-bug/masstransit-rabbitmq-tls-bug/Program.cs:line 37
   at Program.<Main>(String[] args)
```

[ConfigurationHostSettings]: https://github.com/MassTransit/MassTransit/blob/04ac7175e95c733fd58e54aa7ecbf13691b0c7c7/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/Configuration/ConfigurationHostSettings.cs#L21

[RabbitMqHostConfigurator]: https://github.com/MassTransit/MassTransit/blob/04ac7175e95c733fd58e54aa7ecbf13691b0c7c7/src/Transports/MassTransit.RabbitMqTransport/RabbitMqTransport/Configuration/RabbitMqHostConfigurator.cs#L13-L42
