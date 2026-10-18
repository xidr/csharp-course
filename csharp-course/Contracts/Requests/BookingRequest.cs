namespace csharp_course;

/// <summary>
///     Запрос на бронирование события
/// </summary>
/// <param name="Id">Идентификатор события</param>
public record BookingRequest(Guid Id);