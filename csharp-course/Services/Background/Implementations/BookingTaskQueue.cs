using System.Collections.Concurrent;

namespace csharp_course;

public class BookingTaskQueueInMemory : IBookingTaskQueue
{
    private readonly ConcurrentQueue<BookingTask> _queue = new();
    
    public void Enqueue(BookingTask bookingTask)
    {
        _queue.Enqueue(bookingTask);
    }

    public bool TryDequeue(out BookingTask bookingTask)
    {
        return _queue.TryDequeue(out bookingTask);
    }
}