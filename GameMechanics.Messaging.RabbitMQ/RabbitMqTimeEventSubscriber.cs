using System.Text;
using System.Text.Json;
using GameMechanics.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GameMechanics.Messaging.RabbitMQ;

/// <summary>
/// RabbitMQ implementation of time event subscriber.
/// </summary>
public class RabbitMqTimeEventSubscriber : ITimeEventSubscriber
{
    private readonly MessagingOptions _options;
    private readonly ILogger<RabbitMqTimeEventSubscriber> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private IConnection? _connection;
    private IModel? _channel;
    private string? _consumerTag;
    private string? _queueName;
    private bool _disposed;

    public RabbitMqTimeEventSubscriber(
        IOptions<MessagingOptions> options,
        ILogger<RabbitMqTimeEventSubscriber> logger)
    {
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public event EventHandler<TimeEventMessage>? TimeEventReceived;
    public event EventHandler<TimeSkipMessage>? TimeSkipReceived;
    public event EventHandler<CombatStateMessage>? CombatStateReceived;

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;
    public bool IsSubscribed => !string.IsNullOrEmpty(_consumerTag);

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return;

        ValidateCredentials();

        var factory = CreateConnectionFactory();

        for (int retry = 0; retry <= _options.RetryCount; retry++)
        {
            try
            {
                _logger.LogInformation("Connecting subscriber to RabbitMQ at {Host}:{Port}...",
                    _options.HostName, _options.Port);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare the exchange (idempotent)
                _channel.ExchangeDeclare(
                    exchange: _options.TimeEventExchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);

                // Declare a queue for this subscriber
                _queueName = _channel.QueueDeclare(
                    queue: _options.TimeEventQueue,
                    durable: true,
                    exclusive: string.IsNullOrEmpty(_options.TimeEventQueue), // Exclusive if auto-named
                    autoDelete: false).QueueName;

                // Bind queue to exchange
                _channel.QueueBind(
                    queue: _queueName,
                    exchange: _options.TimeEventExchange,
                    routingKey: "");

                _logger.LogInformation("Subscriber connected to RabbitMQ, queue: {Queue}", _queueName);
                return;
            }
            catch (Exception ex) when (retry < _options.RetryCount)
            {
                _logger.LogWarning(ex, "Failed to connect subscriber, retry {Retry}/{MaxRetries}",
                    retry + 1, _options.RetryCount);
                await Task.Delay(_options.RetryDelayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException($"Failed to connect subscriber to RabbitMQ after {_options.RetryCount} retries");
    }

    public Task SubscribeAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        if (IsSubscribed)
        {
            _logger.LogDebug("Already subscribed with consumer tag: {Tag}", _consumerTag);
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += OnMessageReceived;
        consumer.Shutdown += (_, args) =>
        {
            _logger.LogWarning("Consumer shutdown: {Reason}", args.ReplyText);
            _consumerTag = null;
        };

        _consumerTag = _channel!.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Subscribed to time events, consumer: {Tag}", _consumerTag);
        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!IsSubscribed) return Task.CompletedTask;

        try
        {
            _channel?.BasicCancel(_consumerTag);
            _logger.LogInformation("Unsubscribed from time events");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during unsubscribe");
        }
        finally
        {
            _consumerTag = null;
        }

        return Task.CompletedTask;
    }

    private void OnMessageReceived(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            var body = Encoding.UTF8.GetString(e.Body.Span);
            var messageType = e.BasicProperties?.Type;

            _logger.LogDebug("Received message type {Type}: {Body}", messageType, body);

            switch (messageType)
            {
                case nameof(TimeEventMessage):
                    var timeEvent = JsonSerializer.Deserialize<TimeEventMessage>(body, _jsonOptions);
                    if (timeEvent != null)
                    {
                        TimeEventReceived?.Invoke(this, timeEvent);
                    }
                    break;

                case nameof(TimeSkipMessage):
                    var skipEvent = JsonSerializer.Deserialize<TimeSkipMessage>(body, _jsonOptions);
                    if (skipEvent != null)
                    {
                        TimeSkipReceived?.Invoke(this, skipEvent);
                    }
                    break;

                case nameof(CombatStateMessage):
                    var combatEvent = JsonSerializer.Deserialize<CombatStateMessage>(body, _jsonOptions);
                    if (combatEvent != null)
                    {
                        CombatStateReceived?.Invoke(this, combatEvent);
                    }
                    break;

                default:
                    // Try to parse as TimeEventMessage by default
                    var defaultEvent = JsonSerializer.Deserialize<TimeEventMessage>(body, _jsonOptions);
                    if (defaultEvent != null)
                    {
                        TimeEventReceived?.Invoke(this, defaultEvent);
                    }
                    break;
            }

            // Acknowledge the message
            _channel?.BasicAck(e.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing received message");
            
            // Negative acknowledge - message will be requeued
            _channel?.BasicNack(e.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Subscriber is not connected. Call ConnectAsync first.");
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
            ClientProvidedName = _options.ClientName ?? $"Threa.Subscriber.{Environment.MachineName}",
            Ssl = _options.UseSsl ? new SslOption { Enabled = true } : new SslOption { Enabled = false },
            DispatchConsumersAsync = false // Use sync consumer for simplicity
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            await UnsubscribeAsync();
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during RabbitMQ subscriber disposal");
        }
    }
}
