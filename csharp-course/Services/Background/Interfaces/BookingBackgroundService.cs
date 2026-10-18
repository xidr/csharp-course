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
                if (_taskQueue.TryDequeue(out var task))
                {
                    _logger.LogInformation(
                        "Начат процесс генерации брони {TaskId}, время начала обработки: {StartedAt}",
                        task.BookingId, task.StartedAt);

                    // Имитация долгой генерации брони
                    await Task.Delay(TimeSpan.FromMilliseconds(_ms), stoppingToken);
                    
                    _bookingService
                    
                    _logger.LogInformation(
                        "Отчёт {TaskId} сгенерирован успешно", task.Id);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации отчёта");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }

        _logger.LogInformation("ReportGenerationService остановлен");
    }
}