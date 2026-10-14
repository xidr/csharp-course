using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace csharp_course.Tests;

[Trait("Service", "EventService")]
public class EventServiceTests
{
    private static readonly DateTime Jan1 = new(2026, 1, 1, 10, 0, 0);

    private readonly EventService _service = new();

    private static DateTime Day(int month, int day, int hour = 10)
    {
        return new DateTime(2026, month, day, hour, 0, 0);
    }

    /// <summary>Создаёт событие длительностью 2 часа, начинающееся в указанное время</summary>
    private Event Add(string title, DateTime startAt, string? description = null)
    {
        return _service.CreateEvent(title, description, startAt, startAt.AddHours(2));
    }

    private PaginatedResult<Event> Query(
        string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 100)
    {
        return _service.GetAllEvents(title, from, to, page, pageSize);
    }

    private static string[] Titles(PaginatedResult<Event> result)
    {
        return result.Items.Select(e => e.Title).ToArray();
    }

    // ---------- Успешные сценарии ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public void CreateEvent_ValidData_ReturnsEventAndStoresIt()
    {
        var created = _service.CreateEvent("Концерт", "Описание", Jan1, Jan1.AddHours(3));

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Концерт", created.Title);
        Assert.Equal("Описание", created.Description);
        Assert.Equal(Jan1, created.StartAt);
        Assert.Equal(Jan1.AddHours(3), created.EndAt);
        Assert.Same(created, _service.GetEvent(created.Id));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public void CreateEvent_NullDescription_StoresEmptyString()
    {
        var created = _service.CreateEvent("Концерт", null, Jan1, Jan1.AddHours(1));

        Assert.Equal(string.Empty, created.Description);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public void CreateEvent_CalledTwice_GeneratesDifferentIds()
    {
        var first = Add("Первое", Jan1);
        var second = Add("Второе", Jan1);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Read")]
    public void GetAllEvents_NoFilters_ReturnsAllEvents()
    {
        Add("A", Day(1, 1));
        Add("B", Day(1, 2));
        Add("C", Day(1, 3));

        var result = Query();

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Read")]
    public void GetAllEvents_NoEvents_ReturnsEmptyResult()
    {
        var result = Query();

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Read")]
    public void GetAllEvents_ReturnsEventsOrderedByStartDate()
    {
        Add("Позже", Day(3, 1));
        Add("Раньше", Day(1, 1));
        Add("Посередине", Day(2, 1));

        var result = Query();

        Assert.Equal(new[] { "Раньше", "Посередине", "Позже" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Read")]
    public void GetEvent_ExistingId_ReturnsEvent()
    {
        var created = Add("Событие", Jan1);

        var found = _service.GetEvent(created.Id);

        Assert.Equal(created.Id, found.Id);
        Assert.Equal("Событие", found.Title);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_ExistingId_UpdatesAllFields()
    {
        var created = Add("Старое название", Jan1, "Старое описание");
        var newStart = Day(5, 1);
        var newEnd = Day(5, 2);

        _service.UpdateEvent(created.Id, "Новое название", "Новое описание", newStart, newEnd);

        var updated = _service.GetEvent(created.Id);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Новое название", updated.Title);
        Assert.Equal("Новое описание", updated.Description);
        Assert.Equal(newStart, updated.StartAt);
        Assert.Equal(newEnd, updated.EndAt);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_ExistingId_DoesNotCreateNewEvent()
    {
        var created = Add("Событие", Jan1);

        _service.UpdateEvent(created.Id, "Другое", null, Jan1, Jan1.AddHours(1));

        Assert.Equal(1, Query().TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Delete")]
    public void DeleteEvent_ExistingId_RemovesEvent()
    {
        var kept = Add("Останется", Day(1, 1));
        var removed = Add("Будет удалено", Day(1, 2));

        _service.DeleteEvent(removed.Id);

        Assert.Throws<NotFoundException>(() => _service.GetEvent(removed.Id));
        Assert.Equal(new[] { kept.Title }, Titles(Query()));
    }

    // ---------- Фильтрация ----------

    [Theory]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    [InlineData("джаз")]
    [InlineData("ДЖАЗ")]
    [InlineData("ЖаЗ")]
    [InlineData("Концерт джаза")]
    public void GetAllEvents_FilterByTitle_IsCaseInsensitivePartialMatch(string title)
    {
        Add("Концерт джаза", Day(1, 1));
        Add("Лекция по C#", Day(1, 2));

        var result = Query(title);

        Assert.Equal(new[] { "Концерт джаза" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FilterByTitle_NoMatch_ReturnsEmpty()
    {
        Add("Концерт", Day(1, 1));

        var result = Query("несуществующее");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Theory]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetAllEvents_EmptyTitle_DoesNotFilter(string? title)
    {
        Add("A", Day(1, 1));
        Add("B", Day(1, 2));

        var result = Query(title);

        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FilterByFrom_ReturnsEventsStartingNotEarlier()
    {
        Add("Раньше", Day(1, 1));
        Add("Ровно в границу", Day(2, 1));
        Add("Позже", Day(3, 1));

        var result = Query(from: Day(2, 1));

        Assert.Equal(new[] { "Ровно в границу", "Позже" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FilterByTo_ReturnsEventsEndingNotLater()
    {
        Add("Раньше", Day(1, 1)); // заканчивается 1 янв 12:00
        _service.CreateEvent("Ровно в границу", null, Day(2, 1), Day(2, 1, 20)); // заканчивается в 20:00
        Add("Позже", Day(3, 1));

        var result = Query(to: Day(2, 1, 20));

        Assert.Equal(new[] { "Раньше", "Ровно в границу" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FilterByFromAndTo_ReturnsEventsInsideRange()
    {
        Add("До диапазона", Day(1, 1));
        Add("Внутри 1", Day(2, 1));
        Add("Внутри 2", Day(2, 10));
        Add("После диапазона", Day(4, 1));

        var result = Query(from: Day(2, 1), to: Day(2, 28));

        Assert.Equal(new[] { "Внутри 1", "Внутри 2" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_EventStartingInsideButEndingAfterTo_IsExcluded()
    {
        _service.CreateEvent("Выходит за границу", null, Day(2, 1), Day(3, 5));

        var result = Query(from: Day(1, 1), to: Day(2, 28));

        Assert.Empty(result.Items);
    }

    // ---------- Пагинация ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_Pagination_ReturnsRequestedPage()
    {
        for (var i = 1; i <= 7; i++)
            Add($"Событие {i}", Day(1, i));

        var result = Query(page: 2, pageSize: 3);

        Assert.Equal(new[] { "Событие 4", "Событие 5", "Событие 6" }, Titles(result));
        Assert.Equal(7, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(3, result.PageSize);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_Pagination_LastPageMayBeIncomplete()
    {
        for (var i = 1; i <= 7; i++)
            Add($"Событие {i}", Day(1, i));

        var result = Query(page: 3, pageSize: 3);

        Assert.Equal(new[] { "Событие 7" }, Titles(result));
        Assert.Equal(7, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_Pagination_PageBeyondEnd_ReturnsEmptyItemsButKeepsTotal()
    {
        for (var i = 1; i <= 3; i++)
            Add($"Событие {i}", Day(1, i));

        var result = Query(page: 5, pageSize: 3);

        Assert.Empty(result.Items);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_Pagination_PagesDoNotOverlapAndCoverEverything()
    {
        for (var i = 1; i <= 10; i++)
            Add($"Событие {i:D2}", Day(1, i));

        var collected = Enumerable.Range(1, 4)
            .SelectMany(page => Titles(Query(page: page, pageSize: 3)))
            .ToArray();

        Assert.Equal(10, collected.Length);
        Assert.Equal(10, collected.Distinct().Count());
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_Pagination_HugePageNumber_DoesNotOverflow()
    {
        Add("Событие", Jan1);

        var result = Query(page: int.MaxValue, pageSize: 100);

        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    // ---------- Комбинированная фильтрация ----------

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Combined")]
    public void GetAllEvents_CombinedFilters_AppliesAllOfThemWithAnd()
    {
        Add("Джаз зимой", Day(1, 10));
        Add("Джаз весной", Day(4, 10));
        Add("Джаз летом", Day(7, 10));
        Add("Лекция весной", Day(4, 12));

        var result = Query("джаз", Day(3, 1), Day(6, 30));

        Assert.Equal(new[] { "Джаз весной" }, Titles(result));
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Combined")]
    public void GetAllEvents_CombinedFiltersWithPagination_TotalCountIgnoresPaging()
    {
        for (var i = 1; i <= 5; i++)
            Add($"Джаз {i}", Day(4, i));
        Add("Лекция", Day(4, 20));
        Add("Джаз осенью", Day(10, 1));

        var result = Query("джаз", Day(4, 1), Day(4, 30), 2, 2);

        Assert.Equal(new[] { "Джаз 3", "Джаз 4" }, Titles(result));
        Assert.Equal(5, result.TotalCount);
    }

    // ---------- Неуспешные сценарии ----------

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Read")]
    public void GetEvent_NonExistingId_ThrowsNotFoundException()
    {
        Assert.Throws<NotFoundException>(() => _service.GetEvent(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_NonExistingId_ThrowsNotFoundException()
    {
        Assert.Throws<NotFoundException>(() =>
            _service.UpdateEvent(Guid.NewGuid(), "Название", null, Jan1, Jan1.AddHours(1)));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_NonExistingId_DoesNotCreateEvent()
    {
        Assert.Throws<NotFoundException>(() =>
            _service.UpdateEvent(Guid.NewGuid(), "Название", null, Jan1, Jan1.AddHours(1)));

        Assert.Equal(0, Query().TotalCount);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Delete")]
    public void DeleteEvent_NonExistingId_ThrowsNotFoundException()
    {
        Assert.Throws<NotFoundException>(() => _service.DeleteEvent(Guid.NewGuid()));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Create")]
    public void CreateEvent_EndBeforeStart_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _service.CreateEvent("Событие", null, Jan1, Jan1.AddHours(-1)));

        Assert.Equal(0, Query().TotalCount);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Create")]
    public void CreateEvent_EndEqualsStart_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() =>
            _service.CreateEvent("Событие", null, Jan1, Jan1));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_EndBeforeStart_ThrowsValidationException()
    {
        var created = Add("Событие", Jan1);

        Assert.Throws<ValidationException>(() =>
            _service.UpdateEvent(created.Id, "Новое", null, Jan1, Jan1.AddDays(-1)));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_EndBeforeStart_LeavesEventUnchanged()
    {
        var created = Add("Исходное название", Jan1, "Исходное описание");

        Assert.Throws<ValidationException>(() =>
            _service.UpdateEvent(created.Id, "Новое", "Новое", Jan1, Jan1.AddDays(-1)));

        var unchanged = _service.GetEvent(created.Id);
        Assert.Equal("Исходное название", unchanged.Title);
        Assert.Equal("Исходное описание", unchanged.Description);
        Assert.Equal(Jan1, unchanged.StartAt);
        Assert.Equal(Jan1.AddHours(2), unchanged.EndAt);
    }

    // ---------- Граничные случаи: пагинация ----------

    [Theory]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Pagination")]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void GetAllEvents_NonPositivePageOrPageSize_ThrowsArgumentOutOfRangeException(int page, int pageSize)
    {
        Add("Событие", Jan1);

        Assert.Throws<ArgumentOutOfRangeException>(() => Query(page: page, pageSize: pageSize));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_TotalIsExactMultipleOfPageSize_LastPageIsFullAndNextIsEmpty()
    {
        for (var i = 1; i <= 6; i++)
            Add($"Событие {i}", Day(1, i));

        var secondPage = Query(page: 2, pageSize: 3);
        var thirdPage = Query(page: 3, pageSize: 3);

        Assert.Equal(new[] { "Событие 4", "Событие 5", "Событие 6" }, Titles(secondPage));
        Assert.Empty(thirdPage.Items);
        Assert.Equal(6, thirdPage.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_PageSizeEqualsTotal_ReturnsEverythingOnFirstPage()
    {
        for (var i = 1; i <= 3; i++)
            Add($"Событие {i}", Day(1, i));

        var result = Query(page: 1, pageSize: 3);

        Assert.Equal(3, result.Items.Count);
        Assert.Empty(Query(page: 2, pageSize: 3).Items);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_PageSizeOne_ReturnsSingleEventPerPage()
    {
        for (var i = 1; i <= 3; i++)
            Add($"Событие {i}", Day(1, i));

        Assert.Equal(new[] { "Событие 1" }, Titles(Query(page: 1, pageSize: 1)));
        Assert.Equal(new[] { "Событие 3" }, Titles(Query(page: 3, pageSize: 1)));
        Assert.Empty(Query(page: 4, pageSize: 1).Items);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Pagination")]
    public void GetAllEvents_EventsWithSameStartDate_AreOrderedByIdAndPagedWithoutDuplicates()
    {
        for (var i = 1; i <= 5; i++)
            Add($"Событие {i}", Jan1);

        var all = Query();
        var ids = all.Items.Select(e => e.Id).ToArray();
        var pagedIds = Enumerable.Range(1, 3)
            .SelectMany(page => Query(page: page, pageSize: 2).Items)
            .Select(e => e.Id)
            .ToArray();

        Assert.Equal(ids.OrderBy(id => id), ids);
        Assert.Equal(ids, pagedIds);
    }

    // ---------- Граничные случаи: диапазон дат ----------

    [Theory]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void GetAllEvents_FromBoundary_IsInclusiveDownToTheTick(long tickOffset, bool expectedIncluded)
    {
        var start = Day(2, 1);
        Add("Событие", start);

        var result = Query(from: start.AddTicks(tickOffset));

        Assert.Equal(expectedIncluded ? 1 : 0, result.TotalCount);
    }

    [Theory]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void GetAllEvents_ToBoundary_IsInclusiveDownToTheTick(long tickOffset, bool expectedIncluded)
    {
        var start = Day(2, 1);
        var created = Add("Событие", start);

        var result = Query(to: created.EndAt.AddTicks(tickOffset));

        Assert.Equal(expectedIncluded ? 1 : 0, result.TotalCount);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_RangeEqualsEventExactly_IncludesEvent()
    {
        var created = Add("Событие", Day(2, 1));

        var result = Query(from: created.StartAt, to: created.EndAt);

        Assert.Equal(new[] { "Событие" }, Titles(result));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FromEqualsTo_ReturnsNothing()
    {
        var created = Add("Событие", Day(2, 1));

        var result = Query(from: created.StartAt, to: created.StartAt);

        Assert.Empty(result.Items);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_FromAfterTo_ReturnsNothing()
    {
        Add("Событие", Day(2, 1));

        var result = Query(from: Day(12, 1), to: Day(1, 1));

        Assert.Empty(result.Items);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Filtering")]
    public void GetAllEvents_DateKindIsIgnoredWhenComparing()
    {
        var start = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Unspecified);
        Add("Событие", start);

        var utcFrom = DateTime.SpecifyKind(start, DateTimeKind.Utc);

        Assert.Equal(1, Query(from: utcFrom).TotalCount);
    }

    // ---------- Граничные случаи: обновление, удаление, идентификаторы ----------

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Update")]
    public void UpdateEvent_EndEqualsStart_ThrowsValidationExceptionAndLeavesEventUnchanged()
    {
        var created = Add("Исходное название", Jan1);

        Assert.Throws<ValidationException>(() =>
            _service.UpdateEvent(created.Id, "Новое", null, Jan1, Jan1));

        var unchanged = _service.GetEvent(created.Id);
        Assert.Equal("Исходное название", unchanged.Title);
        Assert.Equal(Jan1.AddHours(2), unchanged.EndAt);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Delete")]
    public void DeleteEvent_CalledTwice_SecondCallThrowsNotFoundException()
    {
        var created = Add("Событие", Jan1);
        _service.DeleteEvent(created.Id);

        Assert.Throws<NotFoundException>(() => _service.DeleteEvent(created.Id));
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Read")]
    public void GetEvent_EmptyGuid_ThrowsNotFoundException()
    {
        Add("Событие", Jan1);

        Assert.Throws<NotFoundException>(() => _service.GetEvent(Guid.Empty));
    }
}