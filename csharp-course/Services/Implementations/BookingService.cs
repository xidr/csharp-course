namespace csharp_course;

public class BookingService : IBookingService
{
    IBookingRepository _bookingRepository;
    IEventService _eventService;
    ILogger<BookingService> _logger;
    int _ms;
    
    public BookingService(IBookingRepository bookingRepository, IEventService eventService, ILogger<BookingService> logger, int ms = 1000)
    {
        _bookingRepository = bookingRepository;
        _eventService = eventService;
        _logger = logger;
        _ms = ms;
    }

    public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        if (!_eventService.HasEvent(eventId))
            throw new NotFoundException($"Событие {eventId} не найдено");
        
        var newBookingId = Guid.NewGuid();
        
        var newBooking = new Booking(newBookingId, eventId, Booking.BookingStatus.Pending, DateTime.UtcNow);

        await _bookingRepository.TryAddAsync(newBooking, ct);

        return newBooking;
    }

    public async Task<Booking> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetAsync(bookingId, ct);
        
        if (booking is null)
        {
            throw new NotFoundException($"Бронь с id {bookingId} не найдена");
        }
        
        return booking;
    }

    public Task<bool> TryUpdateBookingByIdAsync(Booking booking, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task ProcessBookingAsync(Guid bookingId, CancellationToken ct)
    {
        var pendingBooking = await GetBookingByIdAsync(bookingId, ct);

        if (pendingBooking.Status != Booking.BookingStatus.Pending)
        {
            throw new InvalidOperationException($"Бронь с id {bookingId} не находится в статусе Pending");
        }
        
        _logger.LogInformation(
            "Начат процесс генерации брони {Id}, время получения: {CreatedAt}, время начала обработки: {StartedAt}",
            pendingBooking.Id, pendingBooking.CreatedAt, DateTime.Now);
        
        await Task.Delay(_ms, ct);
        
        var processedAt = DateTime.Now;
        var processedBooking = pendingBooking with
        {
            Status = Booking.BookingStatus.Confirmed,
            ProcessedAt = processedAt
        };
        
        _logger.LogInformation(
            "Обработка брони {Id} завершена, время завершения: {ProcessedAt}, статус: {Status}",
            processedBooking.Id, processedBooking.ProcessedAt, processedBooking.Status);

        await TryUpdateBookingByIdAsync(processedBooking, ct);
    }
}