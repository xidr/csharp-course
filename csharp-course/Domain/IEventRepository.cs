namespace csharp_course;

public interface IEventRepository
{
    bool TryAdd(Guid newEventId, Event newEvent);
    bool TryRemove(Guid eventId);
    bool TryGet(Guid eventId, out Event? @event);
    bool TryUpdate(Guid eventId, Event newEvent);
    bool TryGetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize, out PaginatedResult<Event> events);
}