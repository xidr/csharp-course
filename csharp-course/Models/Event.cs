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

/// <summary>
/// DTO для создания и обновления события
/// </summary>
public record EventDto : IValidatableObject
{
    /// <summary>
    /// Название мероприятия
    /// </summary>
    [Required(ErrorMessage = "Поле названия мероприятия обязательно для заполнения")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Название мероприятия должно быть от 2 до 100 символов")]
    public string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    [StringLength(1000, MinimumLength = 2,
        ErrorMessage = "Описание мероприятия должно быть от 2 до 1000 символов")]
    public string Description { get; init; } = "";
    
    /// <summary>
    /// Дата и время начала мероприятия
    /// </summary>
    [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
    [Range(typeof(DateTime), "2020-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
    public DateTime StartAt { get; init; }

    /// <summary>
    /// Дата и время завершения мероприятия
    /// </summary>
    [Required(ErrorMessage = "Дата завершения обязательна для заполнения")]
    [Range(typeof(DateTime), "2020-01-01", "2030-12-31", ErrorMessage = "Некорректная дата")]
    public DateTime EndAt { get; init; }

    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "Дата завершения должна быть позже даты начала",
                new[] { nameof(EndAt) });
        }
    }
}