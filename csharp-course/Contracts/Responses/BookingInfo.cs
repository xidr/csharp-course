namespace csharp_course;

/// <summary>
///     Ответ с данными брони
/// </summary>
/// <param name="Id">Идентификатор брони</param>
/// <param name="EventId">Идентификатор события, к которому относится бронь</param>
/// <param name="Status">Текущий статус брони</param>
public record BookingInfo(Guid Id, Guid EventId, Booking.BookingStatus Status);

/// <summary>
///     Методы преобразования броней
/// </summary>
public static class BookingMappings
{
    /// <summary>
    ///     Преобразует бронь в ответ API
    /// </summary>
    /// <param name="e">Бронь</param>
    /// <returns>Ответ с данными брони</returns>
    public static BookingInfo ToResponse(this Booking e)
    {
        return new BookingInfo(e.Id, e.EventId, e.Status);
    }
}