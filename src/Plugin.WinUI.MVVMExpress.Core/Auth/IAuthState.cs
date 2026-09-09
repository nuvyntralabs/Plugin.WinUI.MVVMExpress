namespace Plugin.WinUI.MVVMExpress.Auth;

/// <summary>Authentication flag. Production apps should adapt Plugin.Maui.SecureSession.</summary>
public interface IAuthState
{
    /// <summary>Gets a value indicating whether a user is signed in.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Signed-in display name, if any.</summary>
    string? UserName { get; }

    /// <summary>Signed-in email, if the adapter supplies one.</summary>
    string? Email => null;

    /// <summary>Preferred display name. Defaults to <see cref="UserName"/>.</summary>
    string? DisplayName => UserName;

    /// <summary>Raised after sign-in or sign-out. Default is a no-op event.</summary>
    event EventHandler? Changed
    {
        add { }
        remove { }
    }

    /// <summary>Attempts sign-in.</summary>
    Task<Outcome.Outcome> SignInAsync(string userName, string password, CancellationToken cancellationToken = default);

    /// <summary>Clears the session.</summary>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}

/// <summary>Optional register / reset account operations. Not required for sign-in.</summary>
public interface IAccountService
{
    /// <summary>Creates an account.</summary>
    Task<Outcome.Outcome> RegisterAsync(
        string email,
        string password,
        string? displayName = null,
        CancellationToken cancellationToken = default);

    /// <summary>Resets a password for <paramref name="email"/>.</summary>
    Task<Outcome.Outcome> ResetPasswordAsync(
        string email,
        string newPassword,
        CancellationToken cancellationToken = default);
}
