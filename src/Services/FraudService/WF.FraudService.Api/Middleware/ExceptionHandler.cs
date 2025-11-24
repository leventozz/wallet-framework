using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ValidationException = FluentValidation.ValidationException;

namespace WF.FraudService.Middleware
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

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = httpContext.Response.StatusCode,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

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

            var problemDetails = new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "An error occurred while processing your request.",
                Detail = "Internal Server Error",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}

