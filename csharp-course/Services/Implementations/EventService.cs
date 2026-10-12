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
    public IReadOnlyCollection<Event> GetAllEvents()
    {
        return _events.Values.ToArray();
    }
}