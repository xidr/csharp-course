namespace csharp_course;

/// <summary>
/// Ответ с данными мероприятия
/// </summary>
/// <param name="Id">Идентификатор мероприятия</param>
/// <param name="Title">Название мероприятия</param>
/// <param name="Description">Описание мероприятия</param>
/// <param name="StartAt">Дата и время начала мероприятия</param>
/// <param name="EndAt">Дата и время завершения мероприятия</param>
public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt);
    
    
/// <summary>
/// Методы преобразования событий
/// </summary>
public static class EventMappings
{
    /// <summary>
    /// Преобразует событие в ответ API
    /// </summary>
    /// <param name="e">Событие</param>
    /// <returns>Ответ с данными события</returns>
    public static EventResponse ToResponse(this Event e) =>
        new(e.Id, e.Title, e.Description, e.StartAt, e.EndAt);
}