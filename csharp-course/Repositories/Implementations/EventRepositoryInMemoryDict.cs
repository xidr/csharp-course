using System.Collections.Concurrent;

namespace csharp_course;

/// <summary>
///     Потокобезопасное хранилище событий в памяти на основе <see cref="ConcurrentDictionary{TKey,TValue}" />
/// </summary>
public class EventRepositoryInMemoryDict : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();


    /// <inheritdoc />
    public Task<bool> TryAddAsync(Guid newEventId, Event newEvent, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_events.TryAdd(newEventId, newEvent));
    }


    /// <inheritdoc />
    public Task<Event?> GetAsync(Guid eventId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _events.TryGetValue(eventId, out var foundEvent);
        return Task.FromResult(foundEvent);
    }

    /// <inheritdoc />
    public Task<bool> HasEventAsync(Guid eventId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_events.ContainsKey(eventId));
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateAsync(Guid eventId, Event newEvent, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        while (_events.TryGetValue(eventId, out var current))
            if (_events.TryUpdate(eventId, newEvent, current))
                return Task.FromResult(true);

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<bool> TryRemoveAsync(Guid eventId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_events.TryRemove(eventId, out _));
    }

    /// <inheritdoc />
    public Task<PaginatedResult<Event>> GetAllAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        IEnumerable<Event> events = _events.Values;

        if (!string.IsNullOrWhiteSpace(title))
            events = events.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        if (from.HasValue)
            events = events.Where(e => e.StartAt >= from.Value);

        if (to.HasValue)
            events = events.Where(e => e.EndAt <= to.Value);

        var filtered = events
            .OrderBy(e => e.StartAt)
            .ThenBy(e => e.Id);

        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = filtered.Skip(skip).Take(pageSize).ToArray();

        var result = new PaginatedResult<Event>(items, filtered.Count(), page, pageSize);

        return Task.FromResult(result);
    }
}
