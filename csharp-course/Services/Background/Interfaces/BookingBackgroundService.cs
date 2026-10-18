namespace csharp_course;

public class BookingBackgroundService : BackgroundService
{
    private readonly IBookingTaskQueue _taskQueue;
    private readonly ILogger<BookingBackgroundService> _logger;
    private readonly IBookingService _bookingService;
    private readonly int _ms;

    public BookingBackgroundService(
        IBookingTaskQueue taskQueue,
        ILogger<BookingBackgroundService> logger,
        IBookingService bookingService,
        int ms = 2000)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _bookingService = bookingService;
        _ms = ms;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingBackgroundService запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingBookings = await _bookingService.GetPendingAsync(stoppingToken);
                foreach (var pendingBooking in pendingBookings)
                {
                    _logger.LogInformation(
                        "Начат процесс генерации брони {Id}, время получения: {CreatedAt}, время начала обработки: {StartedAt}",
                        pendingBooking.Id, pendingBooking.CreatedAt, DateTime.Now);
                    
                    await Task.Delay(TimeSpan.FromMilliseconds(_ms), stoppingToken);
                    
                    var processedAt = DateTime.Now;
                    var processedBooking = pendingBooking with
                    {
                        Status = Booking.BookingStatus.Confirmed,
                        ProcessedAt = processedAt
                    };

                    await _bookingService.TryUpdateBookingByIdAsync(processedBooking, stoppingToken);
                    
                    _logger.LogInformation(
                        "Обработка брони {Id} завершена, время завершения: {ProcessedAt}, статус: {Status}",
                        pendingBooking.Id, pendingBooking.ProcessedAt, processedBooking.Status);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "Ошибка при генерации отчёта");
            // }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        _logger.LogInformation("BookingBackgroundService остановлен");
    }
}