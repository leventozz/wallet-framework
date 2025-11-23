using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ValidationException = FluentValidation.ValidationException;

namespace WF.CustomerService.Api.Middleware
{
    public class ExceptionHandler(ILogger<ExceptionHandler> _logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            return exception switch
            {
                ValidationException validationException => await HandleValidationExceptionAsync(
                    httpContext,
                    validationException,
                    cancellationToken),
                _ => await HandleGenericExceptionAsync(
                    httpContext,
                    exception,
                    cancellationToken)
            };
        }

        private async Task<bool> HandleValidationExceptionAsync(
            HttpContext httpContext,
            ValidationException exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpContext.Response.ContentType = "application/json";

            var errors = exception.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToArray());

            var response = new
            {
                statusCode = httpContext.Response.StatusCode,
                message = "Validation failed",
                errors = errors
            };

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        private async Task<bool> HandleGenericExceptionAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            _logger.LogError(
                exception,
                "An unhandled exception occurred. RequestId: {RequestId}",
                httpContext.TraceIdentifier);

            var response = new
            {
                statusCode = httpContext.Response.StatusCode,
                message = "An unhandled exception occurred."
            };

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}

