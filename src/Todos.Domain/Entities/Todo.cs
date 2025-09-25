using Todos.Domain.Events;

namespace Todos.Domain.Entities;

public class Todo
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Todo() { } // For ORM

    public Todo(string title, string? description = null, Priority priority = Priority.Medium)
    {
        Id = Guid.NewGuid();
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Priority = priority;
        IsCompleted = false;
        CreatedAt = DateTime.UtcNow;

        _domainEvents.Add(new TodoCreatedEvent(Id, Title, Description, Priority));
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        var oldTitle = Title;
        Title = title;

        _domainEvents.Add(new TodoUpdatedEvent(Id, oldTitle, Title));
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        _domainEvents.Add(new TodoUpdatedEvent(Id, Description, Description));
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
        _domainEvents.Add(new TodoPriorityChangedEvent(Id, Priority));
    }

    public void Complete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;

        _domainEvents.Add(new TodoCompletedEvent(Id, Title, CompletedAt.Value));
    }

    public void Reopen()
    {
        if (!IsCompleted) return;

        IsCompleted = false;
        CompletedAt = null;

        _domainEvents.Add(new TodoReopenedEvent(Id, Title));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public enum Priority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}