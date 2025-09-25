using RabbitMQ.Client;

namespace Todos.Infrastructure.RabbitMQ;

public interface IRabbitMqConnectionFactory
{
    IConnection Connection { get; }
    bool IsConnected { get; }
}
