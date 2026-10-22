namespace csharp_course;

/// <summary>
///     Ответ с данными брони
/// </summary>
/// <param name="Id">Идентификатор брони</param>
/// <param name="EventId">Идентификатор события, к которому относится бронь</param>
/// <param name="Status">Текущий статус брони</param>
/// <param name="CreatedAt">Дата и время создания брони (UTC)</param>
/// <param name="ProcessedAt">Дата и время обработки брони (UTC); <c>null</c>, пока бронь не обработана</param>
public record BookingInfo(
    Guid Id,
    Guid EventId,
    Booking.BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);

/// <summary>
///     Методы преобразования броней
/// </summary>
public static class BookingMappings
{
    /// <summary>
    ///     Преобразует бронь в ответ API
    /// </summary>
    /// <param name="booking">Бронь</param>
    /// <returns>Ответ с данными брони</returns>
    public static BookingInfo ToResponse(this Booking booking)
    {
        return new BookingInfo(
            booking.Id,
            booking.EventId,
            booking.Status,
            booking.CreatedAt,
            booking.ProcessedAt);
    }
}
