namespace csharp_course;

/// <summary>
///     Реализация сервиса бронирований
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventService _eventService;
    private readonly ILogger<BookingService> _logger;
    private readonly int _ms;

    /// <summary>
    ///     Создаёт сервис бронирований
    /// </summary>
    /// <param name="bookingRepository">Хранилище бронирований</param>
    /// <param name="eventService">Сервис событий, нужен для проверки существования события</param>
    /// <param name="logger">Логгер</param>
    /// <param name="ms">Длительность искусственной задержки при обработке брони, мс</param>
    public BookingService(
        IBookingRepository bookingRepository,
        IEventService eventService,
        ILogger<BookingService> logger,
        int ms = 1000)
    {
        _bookingRepository = bookingRepository;
        _eventService = eventService;
        _logger = logger;
        _ms = ms;
    }

    /// <inheritdoc />
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        if (!_eventService.HasEvent(eventId))
            throw new NotFoundException($"Событие {eventId} не найдено");

        var newBooking = new Booking(Guid.NewGuid(), eventId, Booking.BookingStatus.Pending, DateTime.UtcNow);

        if (!await _bookingRepository.TryAddAsync(newBooking, ct))
            throw new InvalidOperationException($"Не удалось сохранить бронь {newBooking.Id}: такой идентификатор уже существует");

        _logger.LogInformation("Бронь {BookingId} создана для события {EventId}", newBooking.Id, eventId);

        return newBooking;
    }

    /// <inheritdoc />
    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return await _bookingRepository.GetAsync(bookingId, ct)
               ?? throw new NotFoundException($"Бронь {bookingId} не найдена");
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct)
    {
        return _bookingRepository.GetPendingAsync(ct);
    }

    /// <inheritdoc />
    public async Task ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await GetBookingByIdAsync(bookingId, ct);

        if (booking.Status != Booking.BookingStatus.Pending)
        {
            _logger.LogInformation(
                "Бронь {BookingId} уже обработана (статус {Status}), пропускаем", booking.Id, booking.Status);
            return;
        }

        // имитация обращения к внешней системе
        await Task.Delay(TimeSpan.FromMilliseconds(_ms), ct);

        var processed = booking with
        {
            Status = Booking.BookingStatus.Confirmed,
            ProcessedAt = DateTime.UtcNow
        };

        // сохраняем, только если бронь всё ещё ожидает обработки: защита от параллельного изменения
        if (!await _bookingRepository.TryUpdateAsync(processed, Booking.BookingStatus.Pending, ct))
        {
            _logger.LogInformation(
                "Бронь {BookingId} уже изменена другим процессом, результат обработки отброшен", booking.Id);
            return;
        }

        _logger.LogInformation(
            "Бронь {BookingId} обработана: статус {Status}, создана {CreatedAt}, обработана {ProcessedAt}",
            processed.Id, processed.Status, processed.CreatedAt, processed.ProcessedAt);
    }
}
