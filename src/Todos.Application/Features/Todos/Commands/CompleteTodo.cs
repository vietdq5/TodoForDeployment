using MediatR;
using Todos.Application.Common.Interfaces;
using Todos.Domain.Interfaces;

namespace Todos.Application.Features.Todos.Commands;

public record CompleteTodoCommand(Guid Id) : IRequest<CompleteTodoResponse>;

public record CompleteTodoResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime? CompletedAt);

public class CompleteTodoCommandHandler : IRequestHandler<CompleteTodoCommand, CompleteTodoResponse>
{
    private readonly ITodoRepository _repository;
    private readonly IEventBus _eventBus;

    public CompleteTodoCommandHandler(ITodoRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<CompleteTodoResponse> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Todo with id {request.Id} not found");

        todo.Complete();
        await _repository.UpdateAsync(todo, cancellationToken);

        // Publish domain events
        foreach (var domainEvent in todo.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }
        
        todo.ClearDomainEvents();

        return new CompleteTodoResponse(
            todo.Id,
            todo.Title,
            todo.IsCompleted,
            todo.CompletedAt);
    }
}