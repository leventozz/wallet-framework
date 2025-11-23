using FluentValidation;
using MediatR;
using System.Reflection;
using WF.Shared.Contracts.Result;
using ValidationException = FluentValidation.ValidationException;

namespace WF.TransactionService.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    (IEnumerable<IValidator<TRequest>> _validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            var responseType = typeof(TResponse);
            
            if (responseType == typeof(Result))
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var error = Error.Validation("Validation", errorMessage);
                var failureResult = Result.Failure(error);
                return (TResponse)(object)failureResult;
            }
            
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var error = Error.Validation("Validation", errorMessage);
                
                var failureMethod = responseType.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Error) }, null);
                if (failureMethod != null)
                {
                    var failureResult = failureMethod.Invoke(null, new object[] { error });
                    return (TResponse)failureResult!;
                }
            }
            
            throw new ValidationException(failures);
        }

        return await next();
    }
}
