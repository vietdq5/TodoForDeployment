using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Todos.Application.Common.Interfaces;
using Todos.Infrastructure.RabbitMQ;

namespace Todos.Infrastructure.EventBus;

public class RabbitMQEventBusService : IEventBus, IDisposable
{
    private readonly ILogger<RabbitMQEventBusService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly RabbitMQSetting _config;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEventBusService(
        ILogger<RabbitMQEventBusService> logger,
        IOptions<RabbitMQSetting> options,
        IRabbitMqConnectionFactory rabbitMqConnectionFactory)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
        };

        _config = options.Value;
        _connection ??= rabbitMqConnectionFactory.Connection;
        if (_channel == null)
        {
            _channel = _connection.CreateModel();
            // Set QoS to control message prefetch
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            // Declare exchange
            _channel.ExchangeDeclare(exchange: _config.ExchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false);
            // Declare queue
            _channel.QueueDeclare(queue: _config.QueueName, durable: true, exclusive: false, autoDelete: false);
            // Bind queue to exchange
            _channel.QueueBind(queue: _config.QueueName, exchange: _config.ExchangeName, routingKey: _config.RoutingKey);
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var retryCount = 0;
        const int maxRetries = 3;

        while (retryCount <= maxRetries)
        {
            try
            {
                var eventType = @event.GetType();
                var eventTypeName = @event.GetType().Name;
                var message = JsonSerializer.Serialize(@event, eventType, _jsonOptions);
                var body = Encoding.UTF8.GetBytes(message);
                _logger.LogInformation($"Preparing event {eventTypeName} with routing key {_config.RoutingKey}: {message}");
                if (_channel == null)
                {
                    throw new InvalidOperationException("RabbitMQ channel is not initialized.");
                }
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Headers = new Dictionary<string, object>
                {
                    ["EventType"] = eventTypeName,
                    ["Source"] = "TodoApi",
                    ["Version"] = "1.0",
                    ["CorrelationId"] = Guid.NewGuid().ToString()
                };

                _channel.BasicPublish(exchange: _config.ExchangeName, routingKey: _config.RoutingKey, basicProperties: properties, body: body);
                _logger.LogInformation("Successfully published event {EventName} with routing key {RoutingKey} (attempt {Attempt})", eventTypeName, _config.RoutingKey, retryCount + 1);
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to publish event {EventType} (attempt {Attempt}/{MaxRetries})", @event.GetType().Name, retryCount, maxRetries + 1);

                if (retryCount > maxRetries)
                {
                    _logger.LogError(ex, "Failed to publish event {EventType} after {MaxRetries} attempts", @event.GetType().Name, maxRetries + 1);
                    throw;
                }

                // Exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 100), cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}