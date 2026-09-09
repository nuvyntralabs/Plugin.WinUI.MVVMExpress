using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Forms;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Playground.ViewModels;

public sealed class EditViewModel : WinUIFormViewModel
{
    private readonly FormField<string> _name;

    public EditViewModel(INavigator navigator, IDialogs dialogs)
        : base(navigator, dialogs)
    {
        _name = Field("Name", "");
        Bind(_name, nameof(Name), () => SaveCommand.NotifyCanExecuteChanged());
        SaveCommand = new AsyncModelCommand(SaveAsync, () => !string.IsNullOrWhiteSpace(Name));
        BackCommand = new AsyncModelCommand(ct => Navigator!.GoBackAsync(ct));
    }

    public string Name
    {
        get => _name.Value ?? "";
        set => _name.Value = value;
    }

    public AsyncModelCommand SaveCommand { get; }
    public AsyncModelCommand BackCommand { get; }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(80, cancellationToken).ConfigureAwait(true);
        MarkClean();
        await Dialogs!.AlertAsync("Saved", "The item is stored.", cancellationToken: cancellationToken).ConfigureAwait(true);
    }

    protected override Task<IReadOnlyList<ValidationMessage>> CollectErrorsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<ValidationMessage> messages = string.IsNullOrWhiteSpace(Name)
            ? [new ValidationMessage(nameof(Name), "Name is required.")]
            : [];
        return Task.FromResult(messages);
    }
}
