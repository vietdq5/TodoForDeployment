using Microsoft.EntityFrameworkCore;
using Todos.Domain.Entities;
using Todos.Domain.Interfaces;
using Todos.Infrastructure.Data;

namespace Todos.Infrastructure.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<Todo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetByPriorityAsync(Priority priority, CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetCompletedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Todos
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Todo> AddAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        _context.Entry(todo).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var todo = await GetByIdAsync(id, cancellationToken);
        if (todo != null)
        {
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}