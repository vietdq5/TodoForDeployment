namespace Todos.Infrastructure.RabbitMQ;

public class RabbitMQSetting
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "notifications.exchange";
    public string QueueName { get; set; } = "notification.send.notify";
    public string RoutingKey { get; set; } = "notification.send.notify";
}
