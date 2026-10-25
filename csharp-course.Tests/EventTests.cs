using System.ComponentModel.DataAnnotations;

namespace csharp_course.Tests;

[Trait("Domain", "Event")]
public class EventTests
{
    private static readonly DateTime StartAt = new(2026, 11, 1, 18, 0, 0);

    private static Event CreateEvent(int totalSeats = 10)
    {
        return Event.Create(
            Guid.NewGuid(),
            "Концерт",
            StartAt,
            StartAt.AddHours(2),
            null,
            totalSeats);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Create")]
    public void Create_PositiveTotalSeats_InitializesAllSeatsAsAvailable()
    {
        var created = CreateEvent(25);

        Assert.Equal(25, created.TotalSeats);
        Assert.Equal(25, created.AvailableSeats);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Failure")]
    [Trait("Feature", "Create")]
    public void Create_NonPositiveTotalSeats_ThrowsValidationException(int totalSeats)
    {
        Assert.Throws<ValidationException>(() => CreateEvent(totalSeats));
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "ReserveSeats")]
    public void TryReserveSeats_EnoughSeats_DecreasesAvailableSeatsAndReturnsTrue()
    {
        var created = CreateEvent(5);

        var reserved = created.TryReserveSeats(3);

        Assert.True(reserved);
        Assert.Equal(2, created.AvailableSeats);
    }

    [Fact]
    [Trait("Category", "Failure")]
    [Trait("Feature", "ReserveSeats")]
    public void TryReserveSeats_NotEnoughSeats_LeavesSeatsUnchangedAndReturnsFalse()
    {
        var created = CreateEvent(2);

        var reserved = created.TryReserveSeats(3);

        Assert.False(reserved);
        Assert.Equal(2, created.AvailableSeats);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "ReleaseSeats")]
    public void ReleaseSeats_ReservedSeats_IncreasesAvailableSeats()
    {
        var created = CreateEvent(5);
        created.TryReserveSeats(3);

        created.ReleaseSeats(2);

        Assert.Equal(4, created.AvailableSeats);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Concurrency")]
    public async Task TryReserveSeats_ConcurrentCalls_NeverOverbooks()
    {
        var created = CreateEvent(25);

        var attempts = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => created.TryReserveSeats()))
            .ToArray();
        var results = await Task.WhenAll(attempts);

        Assert.Equal(25, results.Count(success => success));
        Assert.Equal(0, created.AvailableSeats);
    }
}
