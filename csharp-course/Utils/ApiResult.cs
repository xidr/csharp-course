namespace csharp_course;

/// <summary>
///     Класс ApiResult с возвращаемыми данными
/// </summary>
/// <typeparam name="T">Тип возвращаемых данных</typeparam>
public class ApiResult<T> : ApiBaseResult
{
    /// <summary>
    ///     Возвращаемые данные метода
    /// </summary>
    public required T Data { get; set; }
}

/// <summary>
///     Класс ApiResult без возвращаемых данных
/// </summary>
public class ApiResult : ApiBaseResult
{
}

/// <summary>
///     Базовый класс с основными возвращаемыми параметрами
/// </summary>
public class ApiBaseResult
{
    /// <summary>
    ///     Дата и время ответа
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}