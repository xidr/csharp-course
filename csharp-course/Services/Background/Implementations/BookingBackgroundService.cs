namespace csharp_course;

/// <summary>
///     Фоновый сервис, периодически обрабатывающий брони в статусе <see cref="Booking.BookingStatus.Pending" />
/// </summary>
public class BookingBackgroundService : BackgroundService
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingBackgroundService> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly SemaphoreSlim _concurrencySemaphore;

    /// <summary>
    ///     Создаёт фоновый сервис обработки броней
    /// </summary>
    /// <param name="logger">Логгер</param>
    /// <param name="bookingService">Сервис бронирований, выполняющий обработку</param>
    /// <param name="pollIntervalMs">Пауза между проверками хранилища на наличие новых броней, мс</param>
    /// <param name="maxConcurrency">Максимальное число одновременно обрабатываемых броней</param>
    public BookingBackgroundService(
        ILogger<BookingBackgroundService> logger,
        IBookingService bookingService,
        int pollIntervalMs = 2000,
        int maxConcurrency = 10)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxConcurrency, 1);
        _concurrencySemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
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

        await Task.WhenAll(pending.Select(booking => ProcessBookingAsync(booking, ct)));
    }

    private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
    {
        await _concurrencySemaphore.WaitAsync(stoppingToken);
        try
        {
            await _bookingService.ProcessBookingAsync(booking.Id, stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Ошибка при обработке брони {BookingId}", booking.Id);
        }
        finally
        {
            _concurrencySemaphore.Release();
        }
    }
}
