namespace csharp_course;

/// <summary>
///     Хранилище событий
/// </summary>
public interface IEventRepository
{
    /// <summary>
    ///     Добавляет новое событие
    /// </summary>
    /// <param name="newEventId">Идентификатор события</param>
    /// <param name="newEvent">Добавляемое событие</param>
    /// <returns><c>true</c>, если событие добавлено; <c>false</c>, если событие с таким идентификатором уже есть</returns>
    bool TryAdd(Guid newEventId, Event newEvent);

    /// <summary>
    ///     Удаляет событие
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <returns><c>true</c>, если событие найдено и удалено; иначе <c>false</c></returns>
    bool TryRemove(Guid eventId);

    /// <summary>
    ///     Ищет событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="event">Найденное событие; <c>null</c>, если оно не найдено</param>
    /// <returns><c>true</c>, если событие найдено; иначе <c>false</c></returns>
    bool TryGet(Guid eventId, out Event? @event);

    /// <summary>
    ///     Заменяет существующее событие новым
    /// </summary>
    /// <param name="eventId">Идентификатор заменяемого события</param>
    /// <param name="newEvent">Новое значение события</param>
    /// <returns><c>true</c>, если событие найдено и заменено; иначе <c>false</c></returns>
    bool TryUpdate(Guid eventId, Event newEvent);

    /// <summary>
    ///     Возвращает страницу событий, подходящих под фильтры.
    ///     События упорядочены по дате начала
    /// </summary>
    /// <param name="title">Часть названия события (без учёта регистра)</param>
    /// <param name="from">События, начинающиеся не раньше указанной даты</param>
    /// <param name="to">События, заканчивающиеся не позже указанной даты</param>
    /// <param name="page">Номер страницы (начиная с 1)</param>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <returns>Пангинированный результат</returns>
    PaginatedResult<Event> TryGetAll(string? title, DateTime? from, DateTime? to, int page, int pageSize);
}