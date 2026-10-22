using System.ComponentModel.DataAnnotations;

namespace csharp_course;

/// <summary>
///     Модель события
/// </summary>
public class Event
{
    private readonly Lock _lock = new();

    /// <summary>
    ///     Создаёт новое событие
    /// </summary>
    /// <param name="id">Уникальный идентификатор события</param>
    /// <param name="title">Название события</param>
    /// <param name="startAt">Дата и время начала события</param>
    /// <param name="endAt">Дата и время окончания события</param>
    /// <param name="description">Описание события, null преобразуется в пустую строку</param>
    /// <param name="totalSeats">Общее количество мест на событии</param>
    /// <exception cref="ValidationException">Дата окончания не позже даты начала</exception>
    public Event(Guid id, string title, DateTime startAt, DateTime endAt, string? description, int totalSeats)
    {
        ValidateDates(startAt, endAt);

        ValidateTotalSeats(totalSeats);

        Id = id;
        Title = title;
        Description = description ?? string.Empty;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }
    /// <summary>
    ///     Создаёт событие с указанным количеством мест
    /// </summary>
    public static Event Create(
        Guid id,
        string title,
        DateTime startAt,
        DateTime endAt,
        string? description,
        int totalSeats)
    {
        return new Event(id, title, startAt, endAt, description, totalSeats);
    }

    /// <summary>
    ///     Уникальный идентификатор события
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     Название события
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    ///     Описание события
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    ///     Дата и время начала события
    /// </summary>
    public DateTime StartAt { get; private set; }

    /// <summary>
    ///     Дата и время окончания события
    /// </summary>
    public DateTime EndAt { get; private set; }

    public int TotalSeats { get; private set; }


    public int AvailableSeats { get; private set; }


    public bool TryReserveSeats(int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        lock (_lock)
        {
            if (AvailableSeats < count) return false;

            AvailableSeats -= count;
            return true;
        }
    }

    public void ReleaseSeats(int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        lock (_lock)
        {
            if (count > TotalSeats - AvailableSeats)
                throw new InvalidOperationException("Нельзя освободить больше мест, чем было зарезервировано");

            AvailableSeats += count;
        }
    }
    internal void UpdateDetails(
        string title, DateTime startAt, DateTime endAt, string? description, int totalSeats)
    {
        ValidateDates(startAt, endAt);
        ValidateTotalSeats(totalSeats);

        lock (_lock)
        {
            var reservedSeats = TotalSeats - AvailableSeats;
            if (totalSeats < reservedSeats)
                throw new ValidationException(
                    $"Общее количество мест не может быть меньше количества уже занятых мест ({reservedSeats})");

            Title = title;
            Description = description ?? string.Empty;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats - reservedSeats;
        }
    }

    private static void ValidateDates(DateTime startAt, DateTime endAt)
    {
        if (endAt <= startAt)
            throw new ValidationException("Дата завершения должна быть позже даты начала");
    }

    private static void ValidateTotalSeats(int totalSeats)
    {
        if (totalSeats <= 0)
            throw new ValidationException("Общее количество мест должно быть больше нуля");
    }
}
