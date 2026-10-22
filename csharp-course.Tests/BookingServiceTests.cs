using Microsoft.Extensions.Logging.Abstractions;

namespace csharp_course.Tests;

[Trait("Service", "BookingService")]
public class BookingServiceTests
{
    private static readonly CancellationToken NoCancel = CancellationToken.None;

    private readonly IEventService _eventService = new EventService(new EventRepositoryInMemoryDict());
    private readonly BookingRepositoryInMemoryDict _bookingRepository = new();
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        // задержка 0 мс: тесты не должны ждать имитацию внешней системы
        _service = CreateService(delayMs: 0);
    }

    private BookingService CreateService(int delayMs) =>
        new(_bookingRepository, _eventService, NullLogger<BookingService>.Instance, delayMs);

    private Event CreateEvent() =>
        _eventService.CreateEventAsync(
                "Событие",
                null,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                100,
                NoCancel)
            .GetAwaiter()
            .GetResult();

    // ---------- Создание брони: успешные сценарии ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public async Task CreateBookingAsync_ExistingEvent_ReturnsPendingBooking()
    {
        var existingEvent = CreateEvent();
        var before = DateTime.UtcNow;

        var booking = await _service.CreateBookingAsync(existingEvent.Id, NoCancel);

        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(existingEvent.Id, booking.EventId);
        Assert.Equal(Booking.BookingStatus.Pending, booking.Status);
        Assert.True(booking.CreatedAt >= before);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public async Task CreateBookingAsync_ExistingEvent_StoresBooking()
    {
        var existingEvent = CreateEvent();

        var booking = await _service.CreateBookingAsync(existingEvent.Id, NoCancel);

        Assert.Equal(booking, await _bookingRepository.GetAsync(booking.Id, NoCancel));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public async Task CreateBookingAsync_SeveralBookingsForSameEvent_AllHaveUniqueIds()
    {
        var existingEvent = CreateEvent();

        var ids = new List<Guid>();
        for (var i = 0; i < 5; i++)
            ids.Add((await _service.CreateBookingAsync(existingEvent.Id, NoCancel)).Id);

        Assert.Equal(5, ids.Distinct().Count());
        Assert.Equal(5, (await _service.GetPendingAsync(NoCancel)).Count);
    }

    // ---------- Создание брони: неуспешные сценарии ----------

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Create")]
    public async Task CreateBookingAsync_NonExistingEvent_ThrowsNotFoundExceptionAndStoresNothing()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateBookingAsync(Guid.NewGuid(), NoCancel));

        Assert.Empty(await _service.GetPendingAsync(NoCancel));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Create")]
    public async Task CreateBookingAsync_DeletedEvent_ThrowsNotFoundException()
    {
        var deletedEvent = CreateEvent();
        await _eventService.DeleteEventAsync(deletedEvent.Id, NoCancel);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateBookingAsync(deletedEvent.Id, NoCancel));
    }

    // ---------- Получение брони ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Get")]
    public async Task GetBookingByIdAsync_ExistingBooking_ReturnsIt()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);

        var found = await _service.GetBookingByIdAsync(created.Id, NoCancel);

        Assert.Equal(created, found);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Get")]
    public async Task GetBookingByIdAsync_NonExistingBooking_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetBookingByIdAsync(Guid.NewGuid(), NoCancel));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Get")]
    public async Task GetBookingByIdAsync_AfterProcessing_ReflectsConfirmedStatus()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);

        await _service.ProcessBookingAsync(created.Id, NoCancel);

        var found = await _service.GetBookingByIdAsync(created.Id, NoCancel);
        Assert.Equal(Booking.BookingStatus.Confirmed, found.Status);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Get")]
    public async Task GetBookingByIdAsync_AfterRejection_ReflectsRejectedStatus()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);
        var rejected = created with { Status = Booking.BookingStatus.Rejected, ProcessedAt = DateTime.UtcNow };

        await _bookingRepository.TryUpdateAsync(rejected, Booking.BookingStatus.Pending, NoCancel);

        var found = await _service.GetBookingByIdAsync(created.Id, NoCancel);
        Assert.Equal(Booking.BookingStatus.Rejected, found.Status);
    }

    // ---------- Обработка брони ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Process")]
    public async Task ProcessBookingAsync_PendingBooking_ConfirmsItAndSetsProcessedAt()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);

        await _service.ProcessBookingAsync(created.Id, NoCancel);

        var processed = await _service.GetBookingByIdAsync(created.Id, NoCancel);
        Assert.Equal(Booking.BookingStatus.Confirmed, processed.Status);
        Assert.NotNull(processed.ProcessedAt);
        Assert.True(processed.ProcessedAt >= processed.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Process")]
    public async Task ProcessBookingAsync_PendingBooking_KeepsOtherFieldsUnchanged()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);

        await _service.ProcessBookingAsync(created.Id, NoCancel);

        var processed = await _service.GetBookingByIdAsync(created.Id, NoCancel);
        Assert.Equal(created.Id, processed.Id);
        Assert.Equal(created.EventId, processed.EventId);
        Assert.Equal(created.CreatedAt, processed.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Process")]
    public async Task ProcessBookingAsync_CalledTwice_SecondCallDoesNothing()
    {
        var created = await _service.CreateBookingAsync(CreateEvent().Id, NoCancel);
        await _service.ProcessBookingAsync(created.Id, NoCancel);
        var afterFirst = await _service.GetBookingByIdAsync(created.Id, NoCancel);

        await _service.ProcessBookingAsync(created.Id, NoCancel);

        Assert.Equal(afterFirst, await _service.GetBookingByIdAsync(created.Id, NoCancel));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Process")]
    public async Task ProcessBookingAsync_NonExistingBooking_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ProcessBookingAsync(Guid.NewGuid(), NoCancel));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Process")]
    public async Task ProcessBookingAsync_CancelledDuringDelay_ThrowsAndLeavesBookingPending()
    {
        var slowService = CreateService(delayMs: 10000);
        var created = await slowService.CreateBookingAsync(CreateEvent().Id, NoCancel);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            slowService.ProcessBookingAsync(created.Id, cts.Token));

        var found = await slowService.GetBookingByIdAsync(created.Id, NoCancel);
        Assert.Equal(Booking.BookingStatus.Pending, found.Status);
        Assert.Null(found.ProcessedAt);
    }

    // ---------- Брони в ожидании ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_ReturnsOnlyUnprocessedBookings()
    {
        var existingEvent = CreateEvent();
        var processed = await _service.CreateBookingAsync(existingEvent.Id, NoCancel);
        var stillPending = await _service.CreateBookingAsync(existingEvent.Id, NoCancel);
        await _service.ProcessBookingAsync(processed.Id, NoCancel);

        var pending = await _service.GetPendingAsync(NoCancel);

        Assert.Equal(new[] { stillPending.Id }, pending.Select(b => b.Id));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_NoBookings_ReturnsEmptyCollection()
    {
        Assert.Empty(await _service.GetPendingAsync(NoCancel));
    }
}
