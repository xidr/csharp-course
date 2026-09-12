using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

/// <summary>
/// Контроллер для управления событиями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase {
    readonly IEventService _eventService;
    
    public EventsController(IEventService eventService) {
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
    public ActionResult<ApiResult<List<Event>>> GetAllEvents() {
        return Ok(new ApiResult<List<Event>> {
            Data = _eventService.GetAllEvents().ToList(),
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Получаем все события"
        });
    }
    
    /// <summary>
    /// Метод возвращает событие по идентификатору
    /// </summary>
    /// <param name="index">Идентификатор события</param>
    /// <response code="200">Возвращается JSON-структура ApiResult с деталями ответа
    /// и HTTP статус-кодом 200 Ok в случае успеха</response>
    /// <response code="404">Возвращается HTTP статус код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult<Event>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)] 
    [Produces("application/json")]
    [HttpGet("{index:guid}")]
    public ActionResult<ApiBaseResult> GetEventByIndex(Guid index)
    {
        var eventToReturn = _eventService.GetEvent(index);

        if (eventToReturn != null) {
            return Ok( new ApiResult<Event>
            {
                Data = eventToReturn,
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Получаем событие по индексу из коллекции"
            });
        }
        else {
            return NotFound( new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Не удалось найти событие по индексу: {index}"
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
    [ProducesResponseType(typeof(ApiResult<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    [HttpPost]
    public ActionResult<ApiBaseResult> Post([FromBody]EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Некорректные данные"
            });
        }
        
        var eventId = _eventService.CreateEvent(eventDto.Title, eventDto.Description, eventDto.StartAt, eventDto.EndAt);

        return CreatedAtAction(nameof(GetEventByIndex), new { id = eventId }, new ApiResult<Guid>
        {
            Data = eventId,
            Success = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Событие успешно создано"
        });
    }
    
    
    /// <summary>
    /// Метод обновляет существующее событие по идентификатору
    /// </summary>
    /// <param name="index">Идентификатор события</param>
    /// <param name="eventDto">Новые данные события</param>
    /// <response code="204">Возвращается HTTP статус-код 204 No Content в случае
    /// успешного обновления события</response>
    /// <response code="400">Возвращается HTTP статус-код 400 Bad Request в случае
    /// некорректных входных данных</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpPut("{index:guid}")]
    public ActionResult<ApiBaseResult> Put(Guid index,[FromBody]EventDto eventDto)
    {
        if (!ModelState.IsValid) {
            return NotFound(new ApiResult {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Некорректные данные"
            });
        }
        
        var eventChanged = _eventService.UpdateEvent(index, eventDto.Title, eventDto.Description, eventDto.StartAt, eventDto.EndAt);

        if (!eventChanged) {
            return NotFound(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = "События по указанному индексу не найдено"
            });
        }
        else {
            return NoContent();
        }

    }
    
    /// <summary>
    /// Метод удаляет событие по идентификатору
    /// </summary>
    /// <param name="index">Идентификатор события</param>
    /// <response code="200">Возвращается JSON-структура с подтверждением удаления
    /// и HTTP статус-кодом 200 OK в случае успеха</response>
    /// <response code="404">Возвращается HTTP статус-код 404 Not Found в случае
    /// отсутствия события</response>
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpDelete("{index:guid}")]
    public ActionResult<ApiBaseResult> Delete(Guid index)
    {
        
        var eventWasDeleted = _eventService.DeleteEvent(index);

        if (eventWasDeleted) {
            return Ok(new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Событие успешно удалено"
            });
        }
        else {
            return NotFound(new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = "Событие по указанному индексу не найдено"
            });
        }
        

    }
    
    
}

