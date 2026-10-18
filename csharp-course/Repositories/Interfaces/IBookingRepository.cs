namespace csharp_course;

/// <summary>
///     Хранилище бронирований
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    ///     Добавляет новую бронь
    /// </summary>
    /// <param name="booking">Добавляемая бронь</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns><c>true</c>, если бронь добавлена; <c>false</c>, если бронь с таким идентификатором уже есть</returns>
    Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default);

    /// <summary>
    ///     Ищет бронь по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Найденная бронь или <c>null</c>, если она не найдена</returns>
    Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    ///     Заменяет сохранённую бронь переданной, только если сохранённая бронь
    ///     сейчас находится в ожидаемом статусе. Бронь определяется по <see cref="Booking.Id"/>
    /// </summary>
    /// <param name="booking">Новое значение брони</param>
    /// <param name="expectedStatus">Статус, в котором бронь должна находиться на момент замены</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>
    ///     <c>true</c>, если бронь заменена; <c>false</c>, если бронь не найдена
    ///     или её статус уже отличается от ожидаемого
    /// </returns>
    Task<bool> TryUpdateAsync(Booking booking, Booking.BookingStatus expectedStatus, CancellationToken ct = default);

    /// <summary>
    ///     Возвращает брони в статусе <see cref="Booking.BookingStatus.Pending"/>,
    ///     от самых старых к самым новым
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Брони, ожидающие обработки</returns>
    Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct = default);
}
