namespace csharp_course;

/// <summary>
/// Сервис для работы с событиями
/// </summary>
public interface IEventService {
    /// <summary>
    /// Создаёт новое событие
    /// </summary>
    /// <param name="title">Название события</param>
    /// <param name="description">Описание события</param>
    /// <param name="startDate">Дата и время начала</param>
    /// <param name="endDate">Дата и время окончания</param>
    /// <returns>Созданное событие</returns>
    Event CreateEvent(string title, string? description, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Удаляет событие
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <returns><c>true</c>, если событие найдено и удалено; иначе <c>false</c></returns>
    void DeleteEvent(Guid eventId);

    /// <summary>
    /// Обновляет существующее событие
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="title">Новое название события</param>
    /// <param name="description">Новое описание события</param>
    /// <param name="startDate">Новая дата и время начала</param>
    /// <param name="endDate">Новая дата и время окончания</param>
    /// <returns><c>true</c>, если событие найдено и обновлено; иначе <c>false</c></returns>
    void UpdateEvent(Guid eventId, string title, string? description, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Возвращает событие по идентификатору
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <returns>Событие или <c>null</c>, если оно не найдено</returns>
    Event GetEvent(Guid eventId);

    /// <summary>
    /// Возвращает страницу событий, удовлетворяющих переданным фильтрам.
    /// Фильтры применяются только если заданы и комбинируются по правилу логического И.
    /// События упорядочены по дате начала
    /// </summary>
    /// <param name="title">Часть названия события (регистронезависимый)</param>
    /// <param name="from">События, начинающиеся не раньше указанной даты</param>
    /// <param name="to">События, заканчивающиеся не позже указанной даты</param>
    /// <param name="page">Номер страницы (начиная с 1)</param>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <returns>Запрошенная страница событий и общее количество подходящих событий</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> или <paramref name="pageSize"/> меньше 1</exception>
    PaginatedResult<Event> GetAllEvents(string? title, DateTime? from, DateTime? to, int page, int pageSize);
}