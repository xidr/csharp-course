using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace csharp_course;

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
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
            : LogLevel.Warning;
        
        _logger.Log(
            logLevel,
            statusCode >= StatusCodes.Status500InternalServerError ? ex : null,
            "Request failed with {StatusCode}: {Message}. Method={Method}, Path={Path}, RequestId={RequestId}",
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
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}