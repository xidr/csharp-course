using Microsoft.AspNetCore.Mvc;

namespace csharp_course;


[ApiController]
[Route("[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    
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