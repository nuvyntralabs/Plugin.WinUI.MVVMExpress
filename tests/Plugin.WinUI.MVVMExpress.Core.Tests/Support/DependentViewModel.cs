using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Support;

public sealed class DependentViewModel : ViewModel
{
    private string? _first;
    private string? _last;
    private string? _withCallback;

    public string? First
    {
        get => _first;
        set
        {
            if (SetProperty(ref _first, value))
            {
                NotifyDependsOn(nameof(First), nameof(FullName));
            }
        }
    }

    public string? Last
    {
        get => _last;
        set
        {
            if (SetProperty(ref _last, value))
            {
                NotifyDependsOn(nameof(Last), nameof(FullName));
            }
        }
    }

    public string FullName => $"{First} {Last}".Trim();

    public string? WithCallback
    {
        get => _withCallback;
        set => SetProperty(ref _withCallback, value, _ => ChangingCalls++, _ => ChangedCalls++);
    }

    public int ChangingCalls { get; private set; }

    public int ChangedCalls { get; private set; }

    public void MarkLoading() => Status = ViewModelStatus.Loading;

    public void MarkRefreshing() => Status = ViewModelStatus.Refreshing;

    public void MarkSaving() => Status = ViewModelStatus.Saving;

    public void MarkIdle() => Status = ViewModelStatus.Idle;
}
