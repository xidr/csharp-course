namespace csharp_course;

/// <summary>
///     Фоновый сервис, периодически обрабатывающий брони в статусе <see cref="Booking.BookingStatus.Pending"/>
/// </summary>
public class BookingBackgroundService : BackgroundService
{
    private readonly ILogger<BookingBackgroundService> _logger;
    private readonly IBookingService _bookingService;
    private readonly TimeSpan _pollInterval;

    /// <summary>
    ///     Создаёт фоновый сервис обработки броней
    /// </summary>
    /// <param name="logger">Логгер</param>
    /// <param name="bookingService">Сервис бронирований, выполняющий обработку</param>
    /// <param name="pollIntervalMs">Пауза между проверками хранилища на наличие новых броней, мс</param>
    public BookingBackgroundService(
        ILogger<BookingBackgroundService> logger,
        IBookingService bookingService,
        int pollIntervalMs = 2000)
    {
        _logger = logger;
        _bookingService = bookingService;
        _pollInterval = TimeSpan.FromMilliseconds(pollIntervalMs);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingBackgroundService запущен");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessPendingAsync(stoppingToken);
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // штатная остановка приложения
        }

        _logger.LogInformation("BookingBackgroundService остановлен");
    }

    /// <summary>
    ///     Обрабатывает все ожидающие брони. Ошибка одной брони не мешает обработке остальных
    /// </summary>
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
                _logger.LogError(ex, "Ошибка при обработке брони {BookingId}", booking.Id);
            }
        }
    }
}
