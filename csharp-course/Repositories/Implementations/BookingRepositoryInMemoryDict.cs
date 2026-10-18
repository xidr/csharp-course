using System.Collections.Concurrent;

namespace csharp_course;

/// <summary>
///     Потокобезопасное хранилище бронирований в памяти на основе <see cref="ConcurrentDictionary{TKey,TValue}" />
/// </summary>
public class BookingRepositoryInMemoryDict : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    /// <inheritdoc />
    public Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(_bookings.TryAdd(booking.Id, booking));
    }

    /// <inheritdoc />
    public Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _bookings.TryGetValue(bookingId, out var foundBooking);

        return Task.FromResult(foundBooking);
    }

    /// <inheritdoc />
    public Task<bool> TryUpdateAsync(
        Booking booking, Booking.BookingStatus expectedStatus, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        while (_bookings.TryGetValue(booking.Id, out var current))
        {
            if (current.Status != expectedStatus)
                return Task.FromResult(false);

            if (_bookings.TryUpdate(booking.Id, booking, current))
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyCollection<Booking> pending = _bookings.Values
            .Where(b => b.Status == Booking.BookingStatus.Pending)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToArray();

        return Task.FromResult(pending);
    }
}