namespace WF.Shared.Contracts.Result
{
    public static class ResultExtensions
    {
        public static Result<T> EnsureExists<T>(this T? value, string name, object id)
        {
            return value is null
                ? Result<T>.Failure(Error.NotFound(name, id))
                : Result<T>.Success(value);
        }
    }
}
