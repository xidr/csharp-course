namespace csharp_course;

public interface IEventService {
    Guid CreateEvent(string title, string description, DateTime startDate, DateTime endDate);
    
    bool DeleteEvent(Guid eventId);
    
    bool UpdateEvent(Guid eventId, string title, string description, DateTime startDate, DateTime endDate);

    Event? GetEvent(Guid eventId);
    
    IEnumerable<Event> GetAllEvents();
}