namespace csharp_course;

/// <summary>
/// Результат постраничной выборки
/// </summary>
/// <typeparam name="T">Тип элементов на странице</typeparam>
/// <param name="Items">Элементы текущей страницы</param>
/// <param name="TotalCount">Общее количество элементов, подходящих под фильтры</param>
/// <param name="Page">Номер текущей страницы</param>
/// <param name="PageSize">Количество элементов на странице</param>
public record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
