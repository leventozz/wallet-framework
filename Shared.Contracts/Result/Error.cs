namespace WF.Shared.Contracts.Result
{
    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

        public static Error NotFound(string name, object id) =>
            new("NotFound", $"{name} with id '{id}' was not found.");

        public static Error Validation(string code, string message) =>
            new(code, message);

        public static Error Conflict(string code, string message) =>
            new(code, message);

        public static Error Failure(string code, string message) =>
            new(code, message);
    }
}
