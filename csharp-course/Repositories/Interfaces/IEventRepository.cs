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
    /// <param name="ct">Токен отмены</param>
    /// <returns><c>true</c>, если событие добавлено; <c>false</c>, если событие с таким идентификатором уже есть</returns>
    Task<bool> TryAddAsync(Guid newEventId, Event newEvent, CancellationToken ct = default);

    /// <summary>
    ///     Удаляет событие
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns><c>true</c>, если событие найдено и удалено; иначе <c>false</c></returns>
    Task<bool> TryRemoveAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    ///     Ищет событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Найденное событие или <c>null</c>, если оно не найдено</returns>
    Task<Event?> GetAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    ///     Проверяет, существует ли событие с указанным идентификатором
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns><c>true</c>, если событие существует; иначе <c>false</c></returns>
    Task<bool> HasEventAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    ///     Заменяет существующее событие новым
    /// </summary>
    /// <param name="eventId">Идентификатор заменяемого события</param>
    /// <param name="newEvent">Новое значение события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns><c>true</c>, если событие найдено и заменено; иначе <c>false</c></returns>
    Task<bool> TryUpdateAsync(Guid eventId, Event newEvent, CancellationToken ct = default);

    /// <summary>
    ///     Возвращает страницу событий, подходящих под фильтры.
    ///     События упорядочены по дате начала
    /// </summary>
    /// <param name="title">Часть названия события (без учёта регистра)</param>
    /// <param name="from">События, начинающиеся не раньше указанной даты</param>
    /// <param name="to">События, заканчивающиеся не позже указанной даты</param>
    /// <param name="page">Номер страницы (начиная с 1)</param>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Пангинированный результат</returns>
    Task<PaginatedResult<Event>> GetAllAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
