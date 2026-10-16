namespace csharp_course;

/// <summary>
///     Исключение, возникающее при отсутствии запрашиваемого ресурса.
///     Глобальный обработчик ошибок преобразует его в ответ 404 Not Found
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    ///     Создаёт исключение с описанием ненайденного ресурса
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    public NotFoundException(string message) : base(message)
    {
    }
}