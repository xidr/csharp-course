namespace csharp_course;

public interface IBookingTaskQueue
{
    void Enqueue(BookingTask bookingTask);
    bool TryDequeue(out BookingTask bookingTask);
}