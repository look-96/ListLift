using System.ComponentModel.DataAnnotations;
using ListLift.Application.ListingDrafts.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ListLift.API.Errors;

internal sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, safeDetail) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API error.");
        }
        else if (exception is DbUpdateConcurrencyException concurrencyException)
        {
            var affectedEntities = string.Join(
                ", ",
                concurrencyException.Entries.Select(entry => $"{entry.Metadata.ClrType.Name} ({entry.State})"));
            logger.LogWarning(
                "Database concurrency conflict for EF entries: {AffectedEntities}. TraceId: {TraceId}",
                affectedEntities,
                httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning("Request rejected with status {StatusCode} ({ExceptionType}).",
                statusCode,
                exception.GetType().Name);
        }

        httpContext.Response.StatusCode = statusCode;
        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: safeDetail,
            instance: httpContext.Request.Path)
            .ExecuteAsync(httpContext);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) => exception switch
    {
        ListingDraftNotFoundException => (StatusCodes.Status404NotFound, "Listing draft not found", "The requested draft was not found."),
        MarketplaceCategoryProviderUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Category lookup unavailable", "Marketplace category lookup is not configured. Configure the required marketplace access first."),
        ValidationException validation => (StatusCodes.Status400BadRequest, "Invalid request", validation.Message),
        InvalidDataException invalidData => (StatusCodes.Status400BadRequest, "Invalid request", invalidData.Message),
        ArgumentException argument => (StatusCodes.Status400BadRequest, "Invalid request", argument.Message),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required."),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Draft changed", "The draft expired or changed while processing this request. Reload the draft and try again."),
        InvalidOperationException state => (StatusCodes.Status409Conflict, "Request conflicts with the current state", state.Message),
        _ => (StatusCodes.Status500InternalServerError, "Server error", "The request could not be completed.")
    };
}
