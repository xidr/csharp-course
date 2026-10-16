using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace csharp_course;

/// <summary>
///     Параметры фильтрации и постраничного вывода списка событий
/// </summary>
public record EventsFilterRequest : IValidatableObject
{
    /// <summary>Поиск по названию (регистронезависимый, частичное совпадение)</summary>
    public string? Title { get; init; }

    /// <summary>События, начинающиеся не раньше указанной даты</summary>
    public DateTime? From { get; init; }

    /// <summary>События, заканчивающиеся не позже указанной даты</summary>
    public DateTime? To { get; init; }

    /// <summary>Номер страницы (начиная с 1)</summary>
    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть не меньше 1")]
    public int Page { get; init; } = 1;

    /// <summary>Количество элементов на странице (от 1 до 100)</summary>
    [DefaultValue(10)]
    [Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100")]
    public int PageSize { get; init; } = 10;

    /// <summary>
    ///     Проверяет, что дата начала диапазона не позже даты его окончания
    /// </summary>
    /// <param name="validationContext">Контекст валидации</param>
    /// <returns>Ошибки валидации; пусто, если данные корректны</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From > To)
            yield return new ValidationResult(
                "Дата 'from' не может быть позже даты 'to'",
                new[] { nameof(From) });
    }
}