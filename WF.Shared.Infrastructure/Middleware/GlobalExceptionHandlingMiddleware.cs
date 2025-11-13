using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WF.Shared.Abstractions.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace WF.Shared.Infrastructure.Middleware
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
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
                NotFoundException notFoundException => await HandleNotFoundExceptionAsync(
                    httpContext,
                    notFoundException,
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

        private async Task<bool> HandleNotFoundExceptionAsync(
            HttpContext httpContext,
            NotFoundException exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            httpContext.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = httpContext.Response.StatusCode,
                message = exception.Message
            };

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
