namespace csharp_course;

using System.ComponentModel.DataAnnotations;

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
    [StringLength(1000,
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