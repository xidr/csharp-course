namespace csharp_course;

public interface IBookingService
{
    
    Event CreateBooking(string title, string? description, DateTime startDate, DateTime endDate);
    
    Event GetBooking(Guid eventId);
    
    void UpdateBooking(Guid eventId, string title, string? description, DateTime startDate, DateTime endDate);
    
    
}