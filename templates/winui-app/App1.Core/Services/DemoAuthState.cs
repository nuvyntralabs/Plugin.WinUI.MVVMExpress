using Plugin.WinUI.MVVMExpress.Auth;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace App1;

/// <summary>In-memory demo session. Production apps should adapt Plugin.Maui.SecureSession.</summary>
public sealed class DemoAuthState : IAuthState
{
    public const string DemoEmail = "demo@mvvmexpress.dev";
    public const string DemoPassword = "secret";

    public bool IsAuthenticated { get; private set; }

    public string? UserName { get; private set; }

    public Task<Result> SignInAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (userName == DemoEmail && password == DemoPassword)
        {
            IsAuthenticated = true;
            UserName = userName;
            return Task.FromResult(Result.Success());
        }

        return Task.FromResult(Result.Failure("E_AUTH", "Invalid credentials."));
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsAuthenticated = false;
        UserName = null;
        return Task.CompletedTask;
    }
}
