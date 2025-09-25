using Microsoft.EntityFrameworkCore;
using Todos.Application.Common.Interfaces;
using Todos.Domain.Events;
using Todos.Domain.Interfaces;
using Todos.Infrastructure.Data;
using Todos.Infrastructure.EventBus;
using Todos.Infrastructure.EventBus.EventHandlers;
using Todos.Infrastructure.RabbitMQ;
using Todos.Infrastructure.Repositories;

namespace Todos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<TodoDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use In-Memory database for development/testing
                options.UseInMemoryDatabase("TodoDb");
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        // Repositories
        services.AddScoped<ITodoRepository, TodoRepository>();

        // Event Bus - Use Enhanced version with retry logic
        services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
        services.AddSingleton<IEventBus, RabbitMQEventBusService>();        
        // Event Handlers
        services.AddTransient<IDomainEventHandler<TodoCreatedEvent>, TodoCreatedEventHandler>();
        services.AddTransient<IDomainEventHandler<TodoCompletedEvent>, TodoCompletedEventHandler>();
        services.AddTransient<IDomainEventHandler<TodoUpdatedEvent>, TodoUpdatedEventHandler>();
        return services;
    }
}