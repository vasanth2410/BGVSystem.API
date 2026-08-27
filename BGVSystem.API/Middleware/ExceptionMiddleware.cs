using System.Text.Json;
using BGVSystem.API.Models;
using BGVSystem.Application.Exceptions;

namespace BGVSystem.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType =
            "application/json";

        var statusCode =
            exception switch
            {
                NotFoundException =>
                    StatusCodes.Status404NotFound,

                ValidationException =>
                    StatusCodes.Status400BadRequest,

                UnauthorizedException =>
                    StatusCodes.Status401Unauthorized,

                ForbiddenException =>
                    StatusCodes.Status403Forbidden,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

        context.Response.StatusCode =
            statusCode;

        var errorMessage = exception.InnerException != null
            ? $"{exception.Message} ({exception.InnerException.Message})"
            : exception.Message;

        var response =
            new ErrorResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = errorMessage,
                Timestamp = DateTime.UtcNow
            };

        var json =
            JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}