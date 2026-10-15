using System.Collections.Concurrent;

namespace csharp_course;

/// <summary>
///     Потокобезопасное хранилище событий в памяти на основе <see cref="ConcurrentDictionary{TKey,TValue}"/>
/// </summary>
public class EventRepositoryDict : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();
    
    
    /// <inheritdoc />
    public bool TryAdd(Guid newEventId, Event newEvent)
    {
        return _events.TryAdd(newEventId, newEvent);
    }

    /// <inheritdoc />
    public bool TryRemove(Guid eventId)
    {
        return _events.TryRemove(eventId, out _); 
    }

    /// <inheritdoc />
    public bool TryGet(Guid eventId, out Event? foundEvent)
    {
        return _events.TryGetValue(eventId, out foundEvent);
    }

    /// <inheritdoc />
    public bool TryUpdate(Guid eventId, Event newEvent)
    {
        while (_events.TryGetValue(eventId, out var current))
        {
            if (_events.TryUpdate(eventId, newEvent, current))
            {
                return true;
            }
        }
        
        return false;
        
    }

    /// <inheritdoc />
    public PaginatedResult<Event> TryGetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize)
    {
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

        return result;
    }


}