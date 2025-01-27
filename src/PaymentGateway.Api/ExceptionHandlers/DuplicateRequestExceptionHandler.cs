using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace PaymentGateway.Api.ExceptionHandlers;

using Core.Exceptions;

public class DuplicateRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // if exception is not DuplicateRequestException then skip exception handling
        if (exception is not DuplicateRequestException duplicateRequestException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Duplicate payment request detected",
            Status = StatusCodes.Status409Conflict,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
