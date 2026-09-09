using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Forms;
using Plugin.WinUI.MVVMExpress.Navigation;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Forms;

public sealed class FormViewModelTests
{
    [Fact]
    public async Task Edit_MarksDirty_AndBlocksNavigation()
    {
        var form = new ProbeForm();
        Assert.False(form.IsDirty);
        Assert.True(await form.CanNavigateAwayAsync());

        form.Title = "changed";
        Assert.True(form.IsDirty);
        Assert.False(await form.CanNavigateAwayAsync());

        var navigator = new InMemoryNavigator(_ => !form.IsDirty) { Current = typeof(ProbeForm) };
        var blocked = await navigator.NavigateToAsync<ProbeViewModel>();
        Assert.False(blocked.IsSuccess);
        Assert.Equal("E_GUARD", blocked.Error?.Code);

        form.MarkClean();
        Assert.True(await form.CanNavigateAwayAsync());
        Assert.True((await navigator.NavigateToAsync<ProbeViewModel>()).IsSuccess);
    }

    [Fact]
    public void UndoRedo_RestoresValues()
    {
        var form = new ProbeForm { Title = "one" };
        form.Title = "two";
        Assert.Equal("two", form.Title);
        form.Undo();
        Assert.Equal("one", form.Title);
        form.Redo();
        Assert.Equal("two", form.Title);
        form.Reset();
        Assert.Equal("", form.Title);
        Assert.False(form.IsDirty);
    }

    [Fact]
    public void DisposedForm_IsCollectable()
    {
        var weak = CreateAndDispose();
        Assert.True(LeakProbe.IsCollected(weak));
    }

    private static WeakReference CreateAndDispose()
    {
        var form = new ProbeForm { Title = "x" };
        form.Dispose();
        return LeakProbe.Track(form);
    }

    private sealed class ProbeForm : FormViewModel
    {
        private readonly FormField<string> _title;

        public ProbeForm()
        {
            _title = Field("Title", "");
            Bind(_title, nameof(Title));
        }

        public string Title
        {
            get => _title.Value ?? "";
            set => _title.Value = value ?? "";
        }
    }

    private sealed class ProbeViewModel : ViewModel;
}
