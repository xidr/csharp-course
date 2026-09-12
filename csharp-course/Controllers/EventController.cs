using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase {
    readonly IEventService _eventService;
    
    public EventsController(IEventService eventService) {
        _eventService = eventService;
    }
    
    [HttpGet]
    public ApiResult<List<Event>> GetAllEvents()
    {
        return new ApiResult<List<Event>>
        {
            Data = _eventService.GetAllEvents().ToList(),
            Success = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Получаем все события"
        };
    }
    
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
    
    [HttpPost]
    public ApiBaseResult Post([FromBody]EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.BadRequest,
                Message = "Некорректные данные"
            };
        }
        
        var eventId = _eventService.CreateEvent(eventDto.Title, eventDto.Description, eventDto.StartAt, eventDto.EndAt);

        return new ApiResult<Guid>
        {
            Data = eventId,
            Success = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Событие успешно создано"
        };
    }
    
    
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
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = "События по указанному индексу не найдено"
            };
        }
        else {
            return new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.NoContent,
                Message = $"Событие {index} успешно изменено"
            };
        }

    }
    
    [HttpDelete("{index:guid}")]
    public ApiBaseResult Delete(Guid index)
    {
        
        var eventWasDeleted = _eventService.DeleteEvent(index);

        if (eventWasDeleted) {
            return new ApiResult
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Событие успешно удалено"
            };
        }
        else {
            return new ApiResult
            {
                Success = false,
                StatusCode = HttpStatusCode.NotFound,
                Message = "Событие по указанному индексу не найдено"
            };
        }
        

    }
    
    
}

