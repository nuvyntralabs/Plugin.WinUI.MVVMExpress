using Plugin.WinUI.MVVMExpress.Auth;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.Playground.Services;

public sealed class DemoAuthState : IAuthState
{
    public bool IsAuthenticated { get; private set; }
    public string? UserName { get; private set; }

    public Task<Result> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (userName == "demo@mvvmexpress.dev" && password == "secret")
        {
            IsAuthenticated = true;
            UserName = userName;
            return Task.FromResult(Result.Success());
        }

        return Task.FromResult(Result.Failure("E_AUTH", "Invalid credentials."));
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        IsAuthenticated = false;
        UserName = null;
        return Task.CompletedTask;
    }
}
