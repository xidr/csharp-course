using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

/// <summary>
///     Методы расширения для подключения глобальной обработки ошибок
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    ///     Добавляет в конвейер middleware глобальной обработки исключений.
    ///     Должен вызываться до маршрутизации, чтобы перехватывать ошибки контроллеров
    /// </summary>
    /// <param name="builder">Построитель конвейера приложения</param>
    /// <returns>Билдер конвейера приложения</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}

/// <summary>
///     Middleware, перехватывающий исключения, не обработанные в контроллерах и сервисах, и
///     возвращающий единообразный ответ в формате Problem Details (RFC 7807)
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    /// <summary>
    ///     Создаёт middleware глобальной обработки ошибок
    /// </summary>
    /// <param name="next">Следующий обработчик в конвейере</param>
    /// <param name="logger">Логгер для записи ошибок</param>
    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    ///     Передаёт запрос дальше по конвейеру и обрабатывает возникшие исключения
    /// </summary>
    /// <param name="httpContext">Контекст текущего HTTP-запроса</param>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (OperationCanceledException)
            when (httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Запрос отменён. Method={Method}, Path={Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            if (!httpContext.Response.HasStarted)
                httpContext.Response.StatusCode =
                    StatusCodes.Status499ClientClosedRequest;
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        var statusCode = MapStatusCode(ex);

        var logLevel = statusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : LogLevel.Information;

        _logger.Log(
            logLevel,
            statusCode >= StatusCodes.Status500InternalServerError ? ex : null,
            "Ошибка с кодом {StatusCode}: {Message}. Method={Method}, Path={Path}, RequestId={RequestId}",
            statusCode,
            ex.Message,
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.Headers["x-request-id"]);


        if (httpContext.Response.HasStarted) return;


        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var error = new ProblemDetails
        {
            Status = statusCode,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "Внутренняя ошибка сервера"
                : ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error, (JsonSerializerOptions?)null, "application/problem+json");
    }

    private static int MapStatusCode(Exception ex)
    {
        return ex switch
        {
            BadHttpRequestException badRequest => badRequest.StatusCode,
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}