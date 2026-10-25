namespace csharp_course.Tests;

[Trait("Service", "Booking")]
public class BookingTests
{
    private static Booking CreatePending() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Booking.BookingStatus.Pending, DateTime.UtcNow);

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public void Confirm_PendingBooking_ReturnsConfirmedWithProcessedAt()
    {
        var booking = CreatePending();

        var confirmed = booking.Confirm();

        Assert.Equal(Booking.BookingStatus.Confirmed, confirmed.Status);
        Assert.NotNull(confirmed.ProcessedAt);
        Assert.Equal(booking.Id, confirmed.Id);
        Assert.Equal(booking.EventId, confirmed.EventId);
        Assert.Equal(booking.CreatedAt, confirmed.CreatedAt);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public void Reject_PendingBooking_ReturnsRejectedWithProcessedAt()
    {
        var booking = CreatePending();

        var rejected = booking.Reject();

        Assert.Equal(Booking.BookingStatus.Rejected, rejected.Status);
        Assert.NotNull(rejected.ProcessedAt);
        Assert.Equal(booking.Id, rejected.Id);
        Assert.Equal(booking.EventId, rejected.EventId);
    }

    [Fact]
    [Trait("Category", "Success")]
    [Trait("Feature", "Update")]
    public void ConfirmAndReject_DoNotModifyOriginalBooking()
    {
        var booking = CreatePending();

        booking.Confirm();
        booking.Reject();

        Assert.Equal(Booking.BookingStatus.Pending, booking.Status);
        Assert.Null(booking.ProcessedAt);
    }
}
