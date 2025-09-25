using MediatR;
using Todos.Domain.Entities;
using Todos.Domain.Interfaces;

namespace Todos.Application.Features.Todos.Queries;

public record GetTodosQuery() : IRequest<GetTodosResponse>;

public record GetTodosResponse(List<TodoDto> Todos);

public record GetTodoByIdQuery(Guid Id) : IRequest<TodoDto?>;

public record GetTodosByPriorityQuery(Priority Priority) : IRequest<GetTodosResponse>;

public record TodoDto(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, GetTodosResponse>
{
    private readonly ITodoRepository _repository;

    public GetTodosQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetTodosResponse> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _repository.GetAllAsync(cancellationToken);
        
        var todoDtos = todos.Select(t => new TodoDto(
            t.Id,
            t.Title,
            t.Description,
            t.Priority,
            t.IsCompleted,
            t.CreatedAt,
            t.CompletedAt)).ToList();

        return new GetTodosResponse(todoDtos);
    }
}

public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoDto?>
{
    private readonly ITodoRepository _repository;

    public GetTodoByIdQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoDto?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        return todo == null ? null : new TodoDto(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.Priority,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt);
    }
}

public class GetTodosByPriorityQueryHandler : IRequestHandler<GetTodosByPriorityQuery, GetTodosResponse>
{
    private readonly ITodoRepository _repository;

    public GetTodosByPriorityQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetTodosResponse> Handle(GetTodosByPriorityQuery request, CancellationToken cancellationToken)
    {
        var todos = await _repository.GetByPriorityAsync(request.Priority, cancellationToken);
        
        var todoDtos = todos.Select(t => new TodoDto(
            t.Id,
            t.Title,
            t.Description,
            t.Priority,
            t.IsCompleted,
            t.CreatedAt,
            t.CompletedAt)).ToList();

        return new GetTodosResponse(todoDtos);
    }
}