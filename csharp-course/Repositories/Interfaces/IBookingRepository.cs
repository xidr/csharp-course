namespace csharp_course;

public interface IBookingRepository
{
    Task<bool> TryAddAsync(Booking booking, CancellationToken ct = default);

    Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct = default);


    Task<bool> TryUpdateAsync(Guid bookingId, Booking booking, CancellationToken ct = default);


    Task<IReadOnlyCollection<Booking>> GetPendingAsync(CancellationToken ct = default);
}