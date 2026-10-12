using System.ComponentModel.DataAnnotations;

namespace csharp_course;

public record EventsFilterRequest : IValidatableObject
{
    /// <summary>Поиск по названию (регистронезависимый, частичное совпадение)</summary>
    public string? Title { get; init; }

    /// <summary>События, начинающиеся не раньше указанной даты</summary>
    public DateTime? From { get; init; }

    /// <summary>События, заканчивающиеся не позже указанной даты</summary>
    public DateTime? To { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From > To)
            yield return new ValidationResult(
                "Дата 'from' не может быть позже даты 'to'",
                new[] { nameof(From) });
    }
}