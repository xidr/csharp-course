using System.Collections.Concurrent;

namespace csharp_course;

public class EventService : IEventService {
    readonly ConcurrentDictionary<Guid, Event> _events = new();
    
    public Event CreateEvent(string title, string description, DateTime startDate, DateTime endDate) {
        var newEventId = Guid.NewGuid();
        var newEvent = new Event(newEventId, title, startDate, endDate, description);
        _events.TryAdd(newEventId, newEvent);
        
        return newEvent;
    }

    public bool DeleteEvent(Guid eventId) {
        return _events.TryRemove(eventId, out _);
    }

    public bool UpdateEvent(Guid eventId, string title, string description, DateTime startDate, DateTime endDate) {
        while (_events.TryGetValue(eventId, out var current))
        {
            var updated = new Event(eventId, title, startDate, endDate, description);

            if (_events.TryUpdate(eventId, updated, current))
                return true;
        }

        return false;
    }

    public Event? GetEvent(Guid eventId) {
        return _events.GetValueOrDefault(eventId);
    }

    public IReadOnlyCollection<Event> GetAllEvents()
    {
        return _events.Values.ToArray();
    }
}