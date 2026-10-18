namespace csharp_course;

/// <summary>
///     Сервис для работы с бронированиями
/// </summary>
public interface IBookingService
{
    /// <summary>
    ///     Создаёт бронь для события. Бронь создаётся в статусе <see cref="Booking.BookingStatus.Pending" />
    ///     и обрабатывается позже в фоне
    /// </summary>
    /// <param name="eventId">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Созданная бронь</returns>
    /// <exception cref="NotFoundException">Событие не найдено (в том числе удалено)</exception>
    Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    ///     Возвращает бронь по идентификатору
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Найденная бронь</returns>
    /// <exception cref="NotFoundException">Бронь не найдена</exception>
    Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    ///     Возвращает брони, ожидающие обработки, от самых старых к самым новым
    /// </summary>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Брони в статусе <see cref="Booking.BookingStatus.Pending" /></returns>
    Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct);

    /// <summary>
    ///     Обрабатывает бронь: имитирует обращение к внешней системе, затем переводит бронь
    ///     в статус <see cref="Booking.BookingStatus.Confirmed" /> и заполняет дату обработки.
    ///     Если бронь уже обработана, ничего не делает
    /// </summary>
    /// <param name="bookingId">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <exception cref="NotFoundException">Бронь не найдена</exception>
    Task ProcessBookingAsync(Guid bookingId, CancellationToken ct);
}