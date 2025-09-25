using Todos.Domain.Entities;

namespace Todos.Domain.Interfaces;

public interface ITodoRepository
{
    Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Todo>> GetByPriorityAsync(Priority priority, CancellationToken cancellationToken = default);
    Task<List<Todo>> GetCompletedAsync(CancellationToken cancellationToken = default);
    Task<List<Todo>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}