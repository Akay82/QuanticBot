using Microsoft.AspNetCore.Mvc;
using Npgsql;
using QuanticApi.Business.Exceptions;

namespace QuanticApi.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected or canceled an in-flight request.
        }
        catch (BusinessRuleException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Business rule validation failed",
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid request", exception.Message);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.RaiseException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Database business rule validation failed",
                exception.MessageText);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled error occurred while processing the request.");
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string? detail = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        });
    }
}
