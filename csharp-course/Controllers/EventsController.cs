using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

/// <summary>
///     Контроллер для управления событиями
/// </summary>
[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;


    /// <summary>
    ///     Создаёт контроллер событий
    /// </summary>
    /// <param name="eventService">Сервис для работы с событиями</param>
    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }


    /// <summary>
    ///     Метод возвращает страницу событий с учётом необязательных фильтров
    /// </summary>
    /// <param name="filter">
    ///     Параметры фильтрации и пагинации: название, дата начала, дата окончания,
    ///     номер страницы и её размер
    /// </param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">
    ///     Возвращается JSON-структура с деталями ответа
    ///     и HTTP статус-кодом 200 Ok в случае успеха
    /// </response>
    /// <response code="400">
    ///     Возвращается HTTP статус-код 400 Bad Request в случае
    ///     некорректных параметров фильтрации или пагинации
    /// </response>
    [ProducesResponseType(typeof(ApiResult<PaginatedResult<EventResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<ActionResult<ApiResult<PaginatedResult<EventResponse>>>> GetAllEvents(
        [FromQuery] EventsFilterRequest filter,
        CancellationToken ct)
    {
        var result = await _eventService.GetAllEventsAsync(
            filter.Title, filter.From, filter.To, filter.Page, filter.PageSize, ct);

        return Ok(new ApiResult<PaginatedResult<EventResponse>>
        {
            Data = new PaginatedResult<EventResponse>(
                result.Items.Select(x => x.ToResponse()).ToList(),
                result.TotalCount,
                result.Page,
                result.PageSize)
        });
    }

    /// <summary>
    ///     Метод возвращает событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">
    ///     Возвращается JSON-структура ApiResult с деталями ответа
    ///     и HTTP статус-кодом 200 Ok в случае успеха
    /// </response>
    /// <response code="404">
    ///     Возвращается HTTP статус код 404 Not Found в случае
    ///     отсутствия события
    /// </response>
    [ProducesResponseType(typeof(ApiResult<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResult<EventResponse>>> GetEventById(Guid id, CancellationToken ct)
    {
        var eventToReturn = await _eventService.GetEventAsync(id, ct);

        return Ok(new ApiResult<EventResponse>
        {
            Data = eventToReturn.ToResponse()
        });
    }

    /// <summary>
    ///     Метод создает новое событие
    /// </summary>
    /// <param name="request">Данные создаваемого события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="201">
    ///     Возвращается JSON-структура с идентификатором созданного события
    ///     и HTTP статус-кодом 201 Created в случае успеха
    /// </response>
    /// <response code="400">
    ///     Возвращается HTTP статус-код 400 Bad Request в случае
    ///     некорректных входных данных
    /// </response>
    [ProducesResponseType(typeof(ApiResult<EventResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult<ApiResult<EventResponse>>> Post(
        [FromBody] EventRequest request,
        CancellationToken ct)
    {
        var newEvent = await _eventService.CreateEventAsync(
            request.Title,
            request.Description,
            request.StartAt!.Value,
            request.EndAt!.Value,
            request.TotalSeats!.Value,
            ct);

        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, new ApiResult<EventResponse>
        {
            Data = newEvent.ToResponse()
        });
    }


    /// <summary>
    ///     Метод обновляет существующее событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="request">Новые данные события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="204">
    ///     Возвращается HTTP статус-код 204 No Content в случае
    ///     успешного обновления события
    /// </response>
    /// <response code="400">
    ///     Возвращается HTTP статус-код 400 Bad Request в случае
    ///     некорректных входных данных
    /// </response>
    /// <response code="404">
    ///     Возвращается HTTP статус-код 404 Not Found в случае
    ///     отсутствия события
    /// </response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Put(Guid id, [FromBody] EventRequest request, CancellationToken ct)
    {
        await _eventService.UpdateEventAsync(
            id,
            request.Title,
            request.Description,
            request.StartAt!.Value,
            request.EndAt!.Value,
            request.TotalSeats!.Value,
            ct);

        return NoContent();
    }

    /// <summary>
    ///     Метод удаляет событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="ct">Токен отмены</param>
    /// <response code="200">
    ///     Возвращается JSON-структура с подтверждением удаления
    ///     и HTTP статус-кодом 200 OK в случае успеха
    /// </response>
    /// <response code="404">
    ///     Возвращается HTTP статус-код 404 Not Found в случае
    ///     отсутствия события
    /// </response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResult>> Delete(Guid id, CancellationToken ct)
    {
        await _eventService.DeleteEventAsync(id, ct);

        return Ok(new ApiResult());
    }
}
