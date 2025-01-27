using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace PaymentGateway.Api.ExceptionHandlers;

using Core.Exceptions;

public class ArgumentExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // if exception is not ArgumentException then skip exception handling
        if (exception is not ArgumentException argumentException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Argument Exception",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
