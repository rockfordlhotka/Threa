using System.Text;
using System.Text.Json;
using GameMechanics.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GameMechanics.Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of time event publisher.
/// </summary>
public class RabbitMqTimeEventPublisher : ITimeEventPublisher
{
    private readonly MessagingOptions _options;
    private readonly ILogger<RabbitMqTimeEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMqTimeEventPublisher(
        IOptions<MessagingOptions> options,
        ILogger<RabbitMqTimeEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return;

        ValidateCredentials();

        var factory = CreateConnectionFactory();

        for (int retry = 0; retry <= _options.RetryCount; retry++)
        {
            try
            {
                _logger.LogInformation("Connecting to RabbitMQ at {Host}:{Port}...", 
                    _options.HostName, _options.Port);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare the exchanges
                _channel.ExchangeDeclare(
                    exchange: _options.TimeEventExchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);

                _channel.ExchangeDeclare(
                    exchange: _options.TimeResultExchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("Connected to RabbitMQ successfully");
                return;
            }
            catch (Exception ex) when (retry < _options.RetryCount)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ, retry {Retry}/{MaxRetries}", 
                    retry + 1, _options.RetryCount);
                await Task.Delay(_options.RetryDelayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to connect to RabbitMQ after {_options.RetryCount} retries");
    }

    public Task PublishTimeEventAsync(TimeEventMessage message, CancellationToken cancellationToken = default)
    {
        return PublishAsync(_options.TimeEventExchange, "time.event", message, cancellationToken);
    }

    public Task PublishTimeSkipAsync(TimeSkipMessage message, CancellationToken cancellationToken = default)
    {
        return PublishAsync(_options.TimeEventExchange, "time.skip", message, cancellationToken);
    }

    public Task PublishCombatStateAsync(CombatStateMessage message, CancellationToken cancellationToken = default)
    {
        return PublishAsync(_options.TimeEventExchange, "combat.state", message, cancellationToken);
    }

    private Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken) 
        where T : TimeMessageBase
    {
        EnsureConnected();

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel!.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // Persistent
        properties.MessageId = message.MessageId.ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = typeof(T).Name;

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        _logger.LogDebug("Published {MessageType} to {Exchange}/{RoutingKey}: {MessageId}",
            typeof(T).Name, exchange, routingKey, message.MessageId);

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Publisher is not connected. Call ConnectAsync first.");
        }
    }

    private void ValidateCredentials()
    {
        if (string.IsNullOrEmpty(_options.UserName) || string.IsNullOrEmpty(_options.Password))
        {
            throw new InvalidOperationException(
                "RabbitMQ credentials are not configured. " +
                "Set Messaging:UserName and Messaging:Password via User Secrets, " +
                "environment variables (Messaging__UserName, Messaging__Password), " +
                "or a secret store like Azure Key Vault.");
        }
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = _options.AutoReconnect,
            ClientProvidedName = _options.ClientName ?? $"Threa.Publisher.{Environment.MachineName}",
            Ssl = _options.UseSsl ? new SslOption { Enabled = true } : new SslOption { Enabled = false }
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during RabbitMQ publisher disposal");
        }

        await Task.CompletedTask;
    }
}
