using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace csharp_course.Tests;

[Trait("Part", "Middleware")]
[Trait("Middleware", "GlobalExceptionHandling")]
public class GlobalExceptionHandlingMiddlewareTests
{
    private static async Task<DefaultHttpContext> RunInstance(Exception exception, Stream body)
    {
        // Arrange
        RequestDelegate next = _ =>
            Task.FromException(exception);

        var middleware = new GlobalExceptionHandlingMiddleware(
            next,
            NullLogger<GlobalExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = body;


        // Act
        await middleware.InvokeAsync(context);

        return context;
    }

    private static async Task AssertContext(DefaultHttpContext context, int statusCode, string contentType, string expectedDetail)
    {
        Assert.Equal(statusCode, context.Response.StatusCode);
        Assert.Equal(contentType, context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(statusCode, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            expectedDetail,
            json.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    [Trait("StatusCode", "404")]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        var exception = new NotFoundException("Событие не найдено");
        using var body = new MemoryStream();

        var runTask = RunInstance(exception, body);

        var context = await runTask;

        // Assert
        await AssertContext(context, StatusCodes.Status404NotFound, "application/problem+json", "Событие не найдено");
    }

    [Fact]
    [Trait("StatusCode", "400")]
    public async Task InvokeAsync_ValidationException_Returns400()
    {
        var exception = new ValidationException("Неверные данные");
        using var body = new MemoryStream();

        var runTask = RunInstance(exception, body);

        var context = await runTask;

        // Assert
        await AssertContext(context, StatusCodes.Status400BadRequest, "application/problem+json", "Неверные данные");
    }

    [Fact]
    [Trait("StatusCode", "500")]
    public async Task InvokeAsync_Exception_Returns500()
    {
        var exception = new Exception("Ужас");
        using var body = new MemoryStream();

        var runTask = RunInstance(exception, body);

        var context = await runTask;

        // Assert
        await AssertContext(context, StatusCodes.Status500InternalServerError, "application/problem+json",
            "Внутренняя ошибка сервера");
    }

    [Theory]
    [InlineData("Invalid request", StatusCodes.Status400BadRequest)]
    [InlineData("Тело запроса слишком большое", StatusCodes.Status413PayloadTooLarge)]
    [InlineData("Неподдерживаемый тип медиа", StatusCodes.Status415UnsupportedMediaType)]
    [InlineData("Обработка сущности невозможна", StatusCodes.Status422UnprocessableEntity)]
    [Trait("StatusCode", "400")]
    public async Task InvokeAsync_BadHttpRequestException_PreservesStatusCode(string message, int statusCode)
    {
        var exception = new BadHttpRequestException(message, statusCode);
        using var body = new MemoryStream();

        var runTask = RunInstance(exception, body);

        var context = await runTask;

        // Assert
        await AssertContext(context, statusCode, "application/problem+json", message);
    }
}