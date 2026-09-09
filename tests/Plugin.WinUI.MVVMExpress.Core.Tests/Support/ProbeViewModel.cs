using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Support;

public sealed class ProbeViewModel : ViewModel
{
    private string? _name;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ModelCommand PingCommand { get; }

    public AsyncModelCommand WaitCommand { get; }

    public int AppearCount { get; private set; }

    public ProbeViewModel()
    {
        PingCommand = new ModelCommand(() => Name = "ping");
        WaitCommand = new AsyncModelCommand(async ct => await Task.Delay(50_000, ct));
    }

    public override Task OnAppearingAsync(CancellationToken cancellationToken = default)
    {
        AppearCount++;
        return Task.CompletedTask;
    }

    /// <summary>Exposes <see cref="ObservableModel.Notify"/> for argument validation tests.</summary>
    public void RaiseNotify(string? propertyName) => Notify(propertyName);

    /// <summary>Exposes <see cref="ObservableModel.SetProperty{T}"/> with a custom comparer.</summary>
    public bool SetName(string? value, IEqualityComparer<string?> comparer)
        => SetProperty(ref _name, value, propertyName: nameof(Name), comparer: comparer);
}
