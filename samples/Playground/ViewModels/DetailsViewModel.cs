using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Playground.ViewModels;

public sealed class DetailsViewModel : PageViewModel, IAcceptNavArgs<DetailsArgs>
{
    private string _title = "Item";

    public DetailsViewModel(INavigator navigator)
        : base(navigator)
    {
        BackCommand = new AsyncModelCommand(ct => Navigator!.GoBackAsync(ct));
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public AsyncModelCommand BackCommand { get; }

    public void Accept(DetailsArgs args) => Title = args.Title;
}
