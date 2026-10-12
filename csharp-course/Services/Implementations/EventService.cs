using System.Collections.Concurrent;

namespace csharp_course;

/// <summary>
/// Потокобезопасная реализация сервиса событий с хранением в памяти
/// </summary>
public class EventService : IEventService {
    readonly ConcurrentDictionary<Guid, Event> _events = new();

    /// <inheritdoc />
    public Event CreateEvent(string title, string? description, DateTime startDate, DateTime endDate) {
        var newEventId = Guid.NewGuid();
        var newEvent = new Event(newEventId, title, startDate, endDate, description);
        _events.TryAdd(newEventId, newEvent);
        
        return newEvent;
    }

    /// <inheritdoc />
    public void DeleteEvent(Guid eventId) {
        if (!_events.TryRemove(eventId, out _))
            throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public void UpdateEvent(Guid eventId, string title, string? description, DateTime startDate, DateTime endDate) {
        while (_events.TryGetValue(eventId, out var current))
        {
            var updated = new Event(eventId, title, startDate, endDate, description);

            if (_events.TryUpdate(eventId, updated, current))
                return;
        }
        
        throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public Event GetEvent(Guid eventId) {
        var foundEvent = _events.GetValueOrDefault(eventId);
        
        if (foundEvent is null)
            throw new NotFoundException($"Событие {eventId} не найдено");
        
        return foundEvent;
    }

    /// <inheritdoc />
    public PaginatedResult<Event> GetAllEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
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
            .ThenBy(e => e.Id)
            .ToArray();

        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = filtered.Skip(skip).Take(pageSize).ToArray();

        return new PaginatedResult<Event>(items, filtered.Length, page, pageSize);
    }
}