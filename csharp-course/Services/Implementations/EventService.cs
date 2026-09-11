namespace csharp_course;

public class EventService : IEventService {
    readonly Dictionary<Guid, Event> _events = new();
    
    public Guid CreateEvent(string title, string description, DateTime startDate, DateTime endDate) {
        var newEventId = Guid.NewGuid();
        var newEvent = new Event(newEventId, title, startDate, endDate, description);
        _events.Add(newEventId, newEvent);
        
        return newEventId;
    }

    public bool DeleteEvent(Guid eventId) {
        return _events.Remove(eventId);
    }

    public bool UpdateEvent(Guid eventId, string title, string description, DateTime startDate, DateTime endDate) {
        if (!_events.TryGetValue(eventId, out var eventToUpdate))
            return false;

        eventToUpdate.UpdateEvent(title, startDate, endDate, description);
        return true;
    }

    public Event? GetEvent(Guid eventId) {
        return _events.GetValueOrDefault(eventId);
    }

    public IEnumerable<Event> GetAllEvents() {
        return _events.Values;
    }
}