namespace csharp_course;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO для создания и обновления события
/// </summary>
public record EventRequest : IValidatableObject
{
    /// <summary>
    /// Название мероприятия
    /// </summary>
    [Required(ErrorMessage = "Поле названия мероприятия обязательно для заполнения")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Название мероприятия должно быть от 2 до 100 символов")]
    public required string Title { get; init; }

    /// <summary>
    /// Описание мероприятия
    /// </summary>
    [StringLength(1000,
        ErrorMessage = "Описание мероприятия должно быть до 1000 символов")]
    public string? Description { get; init; }
    
    /// <summary>
    /// Дата и время начала мероприятия
    /// </summary>
    [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
    public DateTime? StartAt { get; init; }

    /// <summary>
    /// Дата и время завершения мероприятия
    /// </summary>
    [Required(ErrorMessage = "Дата завершения обязательна для заполнения")]
    public DateTime? EndAt { get; init; }

    /// <summary>
    /// Проверяет, что дата завершения позже даты начала
    /// </summary>
    /// <param name="validationContext">Контекст валидации</param>
    /// <returns>Ошибки валидации; пусто, если данные корректны</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        if (EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "Дата завершения должна быть позже даты начала",
                new[] { nameof(EndAt) });
        }
    }
}