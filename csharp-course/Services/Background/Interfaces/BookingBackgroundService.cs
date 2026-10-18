namespace csharp_course;

public class BookingBackgroundService : BackgroundService
{
    private readonly ILogger<BookingBackgroundService> _logger;
    private readonly IBookingService _bookingService;
    private readonly int _ms;

    public BookingBackgroundService(
        ILogger<BookingBackgroundService> logger,
        IBookingService bookingService,
        int ms = 2000)
    {
        _logger = logger;
        _bookingService = bookingService;
        _ms = ms;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingBackgroundService запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessPendingAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // штатная остановка приложения
            }

            _logger.LogInformation("BookingBackgroundService остановлен");
        }
    }
    
    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        IReadOnlyCollection<Booking> pending;
        try
        {
            pending = await _bookingService.GetPendingAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Не удалось получить брони в ожидании");
            return;
        }

        foreach (var booking in pending)
        {
            try
            {
                await _bookingService.ProcessBookingAsync(booking.Id, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Ошибка при обработке брони {Id}", booking.Id);
            }
        }
    }
}