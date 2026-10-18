namespace csharp_course;

public interface IBookingService
{

    Task<Booking?> CreateBookingAsync(Guid eventId, CancellationToken ct);
    
    Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);
    
    Task<bool> TryUpdateBookingByIdAsync(Booking booking, CancellationToken ct);
    
    Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct);
    
}