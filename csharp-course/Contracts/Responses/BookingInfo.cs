namespace csharp_course;

public record BookingInfo(Guid Id, Guid EventId, Booking.BookingStatus Status);

public static class BookingMappings
{
    public static BookingInfo ToResponse(this Booking e)
    {
        return new BookingInfo(e.Id, e.EventId, e.Status);
    }
}