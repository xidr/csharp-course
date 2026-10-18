using System.Collections.Concurrent;

namespace csharp_course;

public class BookingRepositoryInMemoryDict : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_bookings.TryAdd(booking.Id, booking));
    }


    public Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _bookings.TryGetValue(bookingId, out Booking? foundBooking);
        return Task.FromResult(foundBooking);
    }

    public Task<bool> TryUpdateAsync(Guid bookingId, Booking newBookingData, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        
        while (_bookings.TryGetValue(bookingId, out var current))
        {
            
            var updatedBooking = new Booking(current.Id, current.EventId, newBookingData.Status, current.CreatedAt, newBookingData.ProcessedAt);
            if (_bookings.TryUpdate(bookingId, updatedBooking, current))
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
    
    
    public Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyCollection<Booking>>(_bookings.Values.Where(b => b.Status == Booking.BookingStatus.Pending).ToArray());
    }
}