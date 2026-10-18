namespace csharp_course.Tests;

[Trait("Repository", "BookingRepositoryInMemoryDict")]
public class BookingRepositoryInMemoryDictTests
{
    private static readonly DateTime Jan1 = new(2026, 1, 1, 10, 0, 0);

    private readonly BookingRepositoryInMemoryDict _repository = new();

    private static Booking CreateBooking(
        DateTime? createdAt = null,
        Booking.BookingStatus status = Booking.BookingStatus.Pending,
        DateTime? processedAt = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), status, createdAt ?? Jan1, processedAt);

    private async Task<Booking> AddAsync(Booking booking)
    {
        await _repository.TryAddAsync(booking);
        return booking;
    }

    // ---------- Добавление ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Add")]
    public async Task TryAddAsync_NewBooking_ReturnsTrueAndStoresIt()
    {
        var booking = CreateBooking();

        var added = await _repository.TryAddAsync(booking);

        Assert.True(added);
        Assert.Equal(booking, await _repository.GetAsync(booking.Id));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Add")]
    public async Task TryAddAsync_SameIdTwice_ReturnsFalseAndKeepsOriginal()
    {
        var original = await AddAsync(CreateBooking());
        var duplicate = original with { Status = Booking.BookingStatus.Rejected };

        var added = await _repository.TryAddAsync(duplicate);

        Assert.False(added);
        Assert.Equal(original, await _repository.GetAsync(original.Id));
    }

    // ---------- Получение ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Get")]
    public async Task GetAsync_ExistingId_ReturnsBooking()
    {
        var booking = await AddAsync(CreateBooking());

        var found = await _repository.GetAsync(booking.Id);

        Assert.Equal(booking, found);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Get")]
    public async Task GetAsync_NonExistingId_ReturnsNull()
    {
        await AddAsync(CreateBooking());

        var found = await _repository.GetAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    // ---------- Обновление ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public async Task TryUpdateAsync_ExpectedStatusMatches_ReplacesBooking()
    {
        var booking = await AddAsync(CreateBooking());
        var processedAt = Jan1.AddMinutes(5);
        var confirmed = booking with { Status = Booking.BookingStatus.Confirmed, ProcessedAt = processedAt };

        var updated = await _repository.TryUpdateAsync(confirmed, Booking.BookingStatus.Pending);

        Assert.True(updated);
        var stored = await _repository.GetAsync(booking.Id);
        Assert.Equal(Booking.BookingStatus.Confirmed, stored!.Status);
        Assert.Equal(processedAt, stored.ProcessedAt);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public async Task TryUpdateAsync_StatusAlreadyChanged_ReturnsFalseAndKeepsStoredBooking()
    {
        var rejected = await AddAsync(CreateBooking(status: Booking.BookingStatus.Rejected));
        var confirmed = rejected with { Status = Booking.BookingStatus.Confirmed };

        var updated = await _repository.TryUpdateAsync(confirmed, Booking.BookingStatus.Pending);

        Assert.False(updated);
        Assert.Equal(rejected, await _repository.GetAsync(rejected.Id));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public async Task TryUpdateAsync_NonExistingBooking_ReturnsFalseAndStoresNothing()
    {
        var unknown = CreateBooking();

        var updated = await _repository.TryUpdateAsync(unknown, Booking.BookingStatus.Pending);

        Assert.False(updated);
        Assert.Null(await _repository.GetAsync(unknown.Id));
    }

    // ---------- Брони в ожидании ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_ReturnsOnlyPendingBookings()
    {
        var pending = await AddAsync(CreateBooking());
        await AddAsync(CreateBooking(status: Booking.BookingStatus.Confirmed, processedAt: Jan1));
        await AddAsync(CreateBooking(status: Booking.BookingStatus.Rejected, processedAt: Jan1));

        var result = await _repository.GetPendingAsync();

        Assert.Equal(new[] { pending.Id }, result.Select(b => b.Id));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_ReturnsOldestFirst()
    {
        var newest = await AddAsync(CreateBooking(createdAt: Jan1.AddHours(3)));
        var oldest = await AddAsync(CreateBooking(createdAt: Jan1));
        var middle = await AddAsync(CreateBooking(createdAt: Jan1.AddHours(1)));

        var result = await _repository.GetPendingAsync();

        Assert.Equal(new[] { oldest.Id, middle.Id, newest.Id }, result.Select(b => b.Id));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_EmptyStore_ReturnsEmptyCollection()
    {
        var result = await _repository.GetPendingAsync();

        Assert.Empty(result);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pending")]
    public async Task GetPendingAsync_AfterBookingIsConfirmed_NoLongerContainsIt()
    {
        var booking = await AddAsync(CreateBooking());
        await _repository.TryUpdateAsync(
            booking with { Status = Booking.BookingStatus.Confirmed, ProcessedAt = Jan1 },
            Booking.BookingStatus.Pending);

        var result = await _repository.GetPendingAsync();

        Assert.Empty(result);
    }

    // ---------- Отмена ----------

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Cancellation")]
    public async Task AllMethods_CancelledToken_ThrowOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var booking = CreateBooking();

        await Assert.ThrowsAsync<OperationCanceledException>(() => _repository.TryAddAsync(booking, cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => _repository.GetAsync(booking.Id, cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _repository.TryUpdateAsync(booking, Booking.BookingStatus.Pending, cts.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(() => _repository.GetPendingAsync(cts.Token));
    }


}
