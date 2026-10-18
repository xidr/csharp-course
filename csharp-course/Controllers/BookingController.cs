using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

/// <summary>
///     Контроллер для управления бронированиями
/// </summary>
[ApiController]
[Route("bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;


    /// <summary>
    ///     Создаёт контроллер бронирований
    /// </summary>
    /// <param name="bookingService">Сервис для работы с бронированиями</param>
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    ///     Метод создаёт бронь для события. Бронь создаётся в статусе Pending
    ///     и обрабатывается позже в фоновом режиме
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="202">
    ///     Бронь принята к обработке. В теле ответа информация о брони,
    ///     в заголовке Location ссылка на ресурс брони
    /// </response>
    /// <response code="404">
    ///     Возвращается HTTP статус-код 404 Not Found в случае
    ///     отсутствия события
    /// </response>
    [ProducesResponseType(typeof(ApiResult<BookingInfo>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpPost("~/events/{id:guid}/book")]
    public async Task<ActionResult<ApiResult<BookingInfo>>> CreateBooking(Guid id, CancellationToken ct)
    {
        var newBooking = await _bookingService.CreateBookingAsync(id, ct);

        return AcceptedAtAction(nameof(GetBookingInfoById), new { id = newBooking.Id }, new ApiResult<BookingInfo>
        {
            Data = newBooking.ToResponse()
        });
    }

    /// <summary>
    ///     Метод возвращает текущее состояние брони по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор брони</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">
    ///     Возвращается JSON-структура ApiResult с информацией о брони
    ///     и HTTP статус-кодом 200 Ok в случае успеха
    /// </response>
    /// <response code="404">
    ///     Возвращается HTTP статус-код 404 Not Found в случае
    ///     отсутствия брони
    /// </response>
    [ProducesResponseType(typeof(ApiResult<BookingInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResult<BookingInfo>>> GetBookingInfoById(Guid id, CancellationToken ct)
    {
        var bookingToReturn = await _bookingService.GetBookingByIdAsync(id, ct);


        return Ok(new ApiResult<BookingInfo>
        {
            Data = bookingToReturn.ToResponse()
        });
    }
}