namespace csharp_course;

/// <summary>
///     На событии не осталось свободных мест
/// </summary>
public class NoAvailableSeatsException : Exception
{
    /// <summary>
    ///     Создаёт исключение с описанием нехватки мест
    /// </summary>
    public NoAvailableSeatsException(string message) : base(message)
    {
    }
}
