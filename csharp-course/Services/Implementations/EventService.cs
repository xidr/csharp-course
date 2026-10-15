using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace csharp_course;

/// <summary>
///     Реализация сервиса событий, использующая <see cref="IEventRepository"/> для хранения данных
/// </summary>
public class EventService : IEventService
{
    
    private readonly IEventRepository _eventRepository;

    /// <summary>
    ///     Создаёт сервис событий
    /// </summary>
    /// <param name="eventRepository">Хранилище событий</param>
    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public Event CreateEvent(string title, string? description, DateTime startDate, DateTime endDate)
    {
        var newEventId = Guid.NewGuid();
        var newEvent = new Event(newEventId, title, startDate, endDate, description);
        if (_eventRepository.TryAdd(newEventId, newEvent))
        { 
            return newEvent;
        }
        
        throw new InvalidOperationException($"Не удалось создать событие: {newEventId} уже существует");
    }

    /// <inheritdoc />
    public void DeleteEvent(Guid eventId)
    {
        if (!_eventRepository.TryRemove(eventId))
        {
            throw new NotFoundException($"Событие {eventId} не найдено");
        }
    }

    /// <inheritdoc />
    public void UpdateEvent(Guid eventId, string title, string? description, DateTime startDate, DateTime endDate)
    {
        var updatedEvent = new Event(eventId, title, startDate, endDate, description);

        if (_eventRepository.TryUpdate(eventId, updatedEvent))
        {
            return;
        }

        throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public Event GetEvent(Guid eventId)
    {
        if (_eventRepository.TryGet(eventId, out var foundEvent))
        {
            if (foundEvent is null)
                throw new InvalidOperationException($"Событие {eventId} повреждено: значение в хранилище равно null");
            return foundEvent;
        }
        
        throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public PaginatedResult<Event> GetAllEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var result = _eventRepository.TryGetAll(title, from, to, page, pageSize);
        
        return result;
    }
}