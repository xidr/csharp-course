using System.ComponentModel.DataAnnotations;

namespace csharp_course;

/// <summary>
/// Модель события
/// </summary>
public class Event
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Название события
    /// </summary>
    public string Title { get; private set; }
    /// <summary>
    /// Описание события
    /// </summary>
    public string Description { get; private set; }
    /// <summary>
    /// Дата и время начала события
    /// </summary>
    public DateTime StartAt { get; private set; }
    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    public DateTime EndAt { get; private set; }

    public Event(Guid id, string title, DateTime startAt, DateTime endAt, string description = "")
    {
        Id = id;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string description = "")
    {
        
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

}

