using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

/// <summary>
/// Контроллер для управления событиями
/// </summary>
[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventService"></param>
    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }


    /// <summary>
    /// Метод возвращает все события
    /// </summary>
    /// <response code="200">Возвращается JSON-структура с деталями ответа
    /// и HTTP статус-кодом 200 Ok в случае успеха</response>
    [ProducesResponseType(typeof(ApiResult<List<Event>>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet]
    public ActionResult<ApiResult> GetAllEvents()
    {
        return Ok(new ApiResult<List<EventResponse>>
        {
            Data = _eventService.GetAllEvents().Select(x => x.ToResponse()).ToList(),
            Success = true,
        });
    }

    /// <summary>
    /// Метод возвращает событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа
    /// и HTTP статус-кодом 200 Ok в случае успеха</response>
    /// <response code="404">Возвращается HTTP статус код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:guid}")]
    public ActionResult<ApiBaseResult> GetEventById(Guid id)
    {
        var eventToReturn = _eventService.GetEvent(id);

        if (eventToReturn != null)
        {
            return Ok(new ApiResult<EventResponse>
            {
                Data = eventToReturn.ToResponse(),
                Success = true,
            });
        }
        else
        {
            return NotFound(new ApiResult
            {
                Success = false,
            });
        }
    }

    /// <summary>
    /// Метод создает новое событие
    /// </summary>
    /// <param name="eventDto">Данные создаваемого события</param>
    /// <response code="201">Возвращается JSON-структура с идентификатором созданного события
    /// и HTTP статус-кодом 201 Created в случае успеха</response>
    /// <response code="400">Возвращается HTTP статус-код 400 Bad Request в случае
    /// некорректных входных данных</response>
    [ProducesResponseType(typeof(ApiResult<EventResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResultBadRequest), StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    [HttpPost]
    public ActionResult<ApiBaseResult> Post([FromBody] EventDto eventDto)
    {
        var newEvent = _eventService.CreateEvent(eventDto.Title, eventDto.Description, eventDto.StartAt!.Value,
            eventDto.EndAt!.Value);

        return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, new ApiResult<EventResponse>
        {
            Data = newEvent.ToResponse(),
            Success = true
        });
    }


    /// <summary>
    /// Метод обновляет существующее событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <param name="eventDto">Новые данные события</param>
    /// <response code="204">Возвращается HTTP статус-код 204 No Content в случае
    /// успешного обновления события</response>
    /// <response code="400">Возвращается HTTP статус-код 400 Bad Request в случае
    /// некорректных входных данных</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResultBadRequest), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpPut("{id:guid}")]
    public ActionResult<ApiBaseResult> Put(Guid id, [FromBody] EventDto eventDto)
    {
        var eventChanged = _eventService.UpdateEvent(id, eventDto.Title, eventDto.Description, eventDto.StartAt!.Value,
            eventDto.EndAt!.Value);

        if (!eventChanged)
        {
            return NotFound(new ApiResult
            {
                Success = false,
            });
        }
        else
        {
            return NoContent();
        }
    }

    /// <summary>
    /// Метод удаляет событие по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор события</param>
    /// <response code="200">Возвращается JSON-структура с подтверждением удаления
    /// и HTTP статус-кодом 200 OK в случае успеха</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpDelete("{id:guid}")]
    public ActionResult<ApiBaseResult> Delete(Guid id)
    {
        var eventWasDeleted = _eventService.DeleteEvent(id);

        if (eventWasDeleted)
        {
            return Ok(new ApiResult
            {
                Success = true,
            });
        }
        else
        {
            return NotFound(new ApiResult
            {
                Success = false,
            });
        }
    }
}