namespace csharp_course;

/// <summary>
///     Реализация сервиса бронирований
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventService _eventService;
    private readonly ILogger<BookingService> _logger;
    private readonly TimeSpan _processingDelay;
    private readonly SemaphoreSlim _bookingSemaphore = new(1, 1);

    /// <summary>
    ///     Длительность искусственной задержки при обработке брони по умолчанию, мс
    /// </summary>
    public const int DefaultProcessingDelayMs = 1000;

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
        int ms = DefaultProcessingDelayMs)
    {
        _bookingRepository = bookingRepository;
        _eventService = eventService;
        _logger = logger;
        _processingDelay = TimeSpan.FromMilliseconds(ms);
    }

    /// <inheritdoc />
    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        var newBooking = new Booking(Guid.NewGuid(), eventId, Booking.BookingStatus.Pending, DateTime.UtcNow);
        var seatReserved = false;
        var bookingSaved = false;
        var bookedEvent = await _eventService.GetEventAsync(eventId, ct);

        await _bookingSemaphore.WaitAsync(ct);

        try
        {
            seatReserved = bookedEvent.TryReserveSeats();

            if (seatReserved) bookingSaved = await _bookingRepository.TryAddAsync(newBooking, ct);
        }
        finally
        {
            try
            {
                if (seatReserved && !bookingSaved) bookedEvent.ReleaseSeats();
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }

        if (!seatReserved)
            throw new NoAvailableSeatsException("No available seats for this event");

        if (!bookingSaved)
            throw new InvalidOperationException(
                $"Не удалось сохранить бронь {newBooking.Id}: такой идентификатор уже существует");

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

        _logger.LogInformation(
            "Начата обработка брони {BookingId}, поток {ThreadId}", booking.Id, Environment.CurrentManagedThreadId);

        try
        {
            // имитация обращения к внешней системе
            await Task.Delay(_processingDelay, ct);

            await ConfirmOrRejectAsync(booking, ct);
        }
        catch (OperationCanceledException)
        {
            // бронь остаётся Pending и будет обработана позже
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при обработке брони {BookingId}", booking.Id);
            await RejectAndReleaseSeatAsync(booking);
        }
    }

    private async Task ConfirmOrRejectAsync(Booking booking, CancellationToken ct)
    {
        if (!await _eventService.HasEventAsync(booking.EventId, ct))
        {
            if (await _bookingRepository.TryUpdateAsync(booking.Reject(), Booking.BookingStatus.Pending, ct))
                _logger.LogWarning("Бронь {BookingId} отклонена: событие {EventId} не найдено", booking.Id, booking.EventId);

            return;
        }

        var processed = booking.Confirm();

        if (!await _bookingRepository.TryUpdateAsync(processed, Booking.BookingStatus.Pending, ct))
        {
            _logger.LogInformation("Бронь {BookingId} уже изменена другим процессом, результат обработки отброшен", booking.Id);
            return;
        }

        _logger.LogInformation("Бронь {BookingId} обработана: статус {Status}, создана {CreatedAt}, обработана {ProcessedAt}",
            processed.Id, processed.Status, processed.CreatedAt, processed.ProcessedAt);
    }

    private async Task RejectAndReleaseSeatAsync(Booking booking)
    {
        try
        {
            if (!await _bookingRepository.TryUpdateAsync(
                    booking.Reject(), Booking.BookingStatus.Pending, CancellationToken.None))
                return;

            if (await _eventService.HasEventAsync(booking.EventId, CancellationToken.None))
                (await _eventService.GetEventAsync(booking.EventId, CancellationToken.None)).ReleaseSeats();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отклонить бронь {BookingId} и вернуть место", booking.Id);
        }
    }
}