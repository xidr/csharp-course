namespace csharp_course;

/// <summary>
///     Модель брони события
/// </summary>
public record Booking
{
    /// <summary>
    ///     Статус брони
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>Бронь создана и ожидает обработки</summary>
        Pending,

        /// <summary>Бронь подтверждена</summary>
        Confirmed,

        /// <summary>Бронь отклонена</summary>
        Rejected
    }

    /// <summary>
    ///     Создаёт бронь
    /// </summary>
    /// <param name="id">Уникальный идентификатор брони</param>
    /// <param name="eventId">Идентификатор события, для которого делается бронь</param>
    /// <param name="status">Статус брони</param>
    /// <param name="createdAt">Дата и время создания брони (UTC)</param>
    /// <param name="processedAt">Дата и время обработки брони (UTC); <c>null</c>, если бронь ещё не обработана</param>
    public Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt = null)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    /// <summary>
    ///     Уникальный идентификатор брони
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Уникальный идентификатор события
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    ///     Текущий статус брони
    /// </summary>
    public BookingStatus Status { get; init; }

    /// <summary>
    ///     Дата и время создания брони (UTC)
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    ///     Дата и время обработки брони (UTC); <c>null</c>, пока бронь не обработана
    /// </summary>
    public DateTime? ProcessedAt { get; init; }
}