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
    public ActionResult<ApiResult<PaginatedResult<EventResponse>>> GetAllEvents([FromQuery] EventsFilterRequest filter)
    {
        var result = _eventService.GetAllEvents(
            filter.Title, filter.From, filter.To, filter.Page, filter.PageSize);

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
    public ActionResult<ApiResult<EventResponse>> GetEventById(Guid id)
    {
        var eventToReturn = _eventService.GetEvent(id);

        return Ok(new ApiResult<EventResponse>
        {
            Data = eventToReturn.ToResponse()
        });
    }

    /// <summary>
    ///     Метод создает новое событие
    /// </summary>
    /// <param name="request">Данные создаваемого события</param>
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
    public ActionResult<ApiResult<EventResponse>> Post([FromBody] EventRequest request)
    {
        var newEvent = _eventService.CreateEvent(request.Title, request.Description, request.StartAt!.Value,
            request.EndAt!.Value, request.TotalSeats!.Value);

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
    public ActionResult Put(Guid id, [FromBody] EventRequest request)
    {
        _eventService.UpdateEvent(id, request.Title, request.Description, request.StartAt!.Value,
            request.EndAt!.Value, request.TotalSeats!.Value);

        return NoContent();
    }

    /// <summary>
    ///     Метод удаляет событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
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
    public ActionResult<ApiResult> Delete(Guid id)
    {
        _eventService.DeleteEvent(id);

        return Ok(new ApiResult());
    }
}
