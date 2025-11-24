namespace WF.TransactionService.Infrastructure.PropagationContext;

public class UserContext
{
    private static readonly AsyncLocal<string?> _userId = new();

    public string? UserId => _userId.Value;

    public IDisposable SetUser(string? userId)
    {
        var previousUserId = _userId.Value;
        _userId.Value = userId;
        return new UserIdScope(previousUserId);
    }

    private static void RestoreUser(string? previousUserId)
    {
        _userId.Value = previousUserId;
    }

    private readonly struct UserIdScope(string? previousUserId) : IDisposable
    {
        public void Dispose()
        {
            UserContext.RestoreUser(previousUserId);
        }
    }
}

