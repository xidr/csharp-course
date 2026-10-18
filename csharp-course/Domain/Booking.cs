using System.ComponentModel.DataAnnotations;

namespace csharp_course;

/// <summary>
///     Модель события
/// </summary>
public record Booking
{
    
    /// <summary>
    ///     Уникальный идентификатор события
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Уникальный идентификатор события
    /// </summary>
    public Guid EventId { get; init; }

    public BookingStatus Status { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? ProcessedAt { get; init; }
    
    /// <summary>
    ///     Создаёт новую бронь
    ///     Изначально статус брони - Pending, дата создания - текущая локальная дата
    /// </summary>
    /// <param name="id">Уникальный идентификатор брони</param>
    /// <param name="eventId">Идентификатор события, для которого делается бронь</param>
    public Booking(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        EventId = eventId;
        Status = status;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}