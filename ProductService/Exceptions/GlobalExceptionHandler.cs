using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace ProductService.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = "Validation hatası.",
                errors = ((ValidationException)exception).Errors
                    .Select(x => x.ErrorMessage)
            }, cancellationToken);

            return true;
        }

        return false;
    }
}