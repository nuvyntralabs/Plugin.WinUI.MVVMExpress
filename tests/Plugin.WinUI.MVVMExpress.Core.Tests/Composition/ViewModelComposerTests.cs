using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Plugin.WinUI.MVVMExpress.Composition;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Composition;

public sealed class ViewModelComposerTests
{
    [Fact]
    public async Task Attach_PropagatesInitialize_AndDispose()
    {
        var parent = new ParentViewModel();
        var child = parent.Attach(new ChildViewModel());
        await parent.InitializeAsync();
        Assert.Equal(1, child.InitCount);
        Assert.Single(parent.Children);
        parent.Dispose();
        Assert.True(child.IsDisposed);
        Assert.True(child.ViewModelCancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void ScopeFactory_ResolvesTransientViewModel()
    {
        using var provider = new ServiceCollection()
            .AddMvvmExpress()
            .AddTransient<ChildViewModel>()
            .BuildServiceProvider();
        var factory = provider.GetRequiredService<IViewModelScopeFactory>();
        using var scope = factory.CreatePageScope();
        var vm = scope.GetViewModel<ChildViewModel>();
        Assert.NotNull(vm);
        var child = factory.CreateChildScope(scope);
        Assert.NotSame(scope, child);
        child.Dispose();
    }

    [Fact]
    public void DisposedParent_CollectsChild()
    {
        var weak = CreateTree();
        Assert.True(LeakProbe.IsCollected(weak));
    }

    private static WeakReference CreateTree()
    {
        var parent = new ParentViewModel();
        parent.Attach(new ChildViewModel());
        parent.Dispose();
        return LeakProbe.Track(parent);
    }

    private sealed class ParentViewModel : ViewModel
    {
        public override Task InitializeAsync(CancellationToken cancellationToken = default)
            => InitializeChildrenAsync(cancellationToken);
    }

    private sealed class ChildViewModel : ViewModel
    {
        public int InitCount { get; private set; }

        public override Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            InitCount++;
            return Task.CompletedTask;
        }
    }
}
