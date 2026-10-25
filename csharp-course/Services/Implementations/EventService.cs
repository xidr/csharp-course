namespace csharp_course;

/// <summary>
///     Реализация сервиса событий, использующая <see cref="IEventRepository" /> для хранения данных
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
    public async Task<Event> CreateEventAsync(
        string title,
        string? description,
        DateTime startDate,
        DateTime endDate,
        int totalSeats,
        CancellationToken ct)
    {
        var newEventId = Guid.NewGuid();
        var newEvent = Event.Create(newEventId, title, startDate, endDate, description, totalSeats);
        if (await _eventRepository.TryAddAsync(newEventId, newEvent, ct)) return newEvent;

        throw new InvalidOperationException($"Не удалось создать событие: {newEventId} уже существует");
    }

    /// <inheritdoc />
    public async Task DeleteEventAsync(Guid eventId, CancellationToken ct)
    {
        if (!await _eventRepository.TryRemoveAsync(eventId, ct))
            throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public async Task UpdateEventAsync(
        Guid eventId,
        string title,
        string? description,
        DateTime startDate,
        DateTime endDate,
        int totalSeats,
        CancellationToken ct)
    {
        var updatedEvent = await GetEventAsync(eventId, ct);
        updatedEvent.UpdateDetails(title, startDate, endDate, description, totalSeats);

        if (await _eventRepository.TryUpdateAsync(eventId, updatedEvent, ct)) return;

        throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public async Task<Event> GetEventAsync(Guid eventId, CancellationToken ct)
    {
        return await _eventRepository.GetAsync(eventId, ct)
               ?? throw new NotFoundException($"Событие {eventId} не найдено");
    }

    /// <inheritdoc />
    public Task<bool> HasEventAsync(Guid eventId, CancellationToken ct)
    {
        return _eventRepository.HasEventAsync(eventId, ct);
    }

    /// <inheritdoc />
    public Task<PaginatedResult<Event>> GetAllEventsAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return _eventRepository.GetAllAsync(title, from, to, page, pageSize, ct);
    }
}
