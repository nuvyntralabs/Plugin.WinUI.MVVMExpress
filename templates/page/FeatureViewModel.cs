using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace App1;

public sealed class FeatureViewModel : PageViewModel
{
    private readonly IFeatureService _service;
    private string _title = "Feature";
    private int _count;

    public FeatureViewModel(IFeatureService service, INavigator navigator, IDialogs dialogs)
        : base(navigator, dialogs)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
        IncrementCommand = new AsyncModelCommand(IncrementAsync);
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public int Count
    {
        get => _count;
        private set => SetProperty(ref _count, value);
    }

    public AsyncModelCommand IncrementCommand { get; }

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        => Title = await _service.LoadAsync(cancellationToken).ConfigureAwait(true);

    private async Task IncrementAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(40, cancellationToken).ConfigureAwait(true);
        Count++;
    }
}
