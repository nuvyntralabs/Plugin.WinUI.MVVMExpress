using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Core.Tests.Support;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.ComponentModel;

public sealed class ObservableModelTests
{
    [Fact]
    public void SetProperty_RaisesChanged_OnlyWhenValueDiffers()
    {
        var vm = new ProbeViewModel();
        var count = 0;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ProbeViewModel.Name))
            {
                count++;
            }
        };

        vm.Name = "a";
        vm.Name = "a";
        vm.Name = "b";

        Assert.Equal(2, count);
    }

    [Fact]
    public void SameValueSetProperty_DoesNotAllocateEventArgsAfterWarmup()
    {
        var vm = new ProbeViewModel { Name = "warm" };
        vm.PropertyChanged += (_, _) => { };

        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10_000; i++)
        {
            vm.Name = "warm";
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(allocated < 1_024, $"Same-value SetProperty allocated {allocated} bytes.");
    }

    [Fact]
    public void SetProperty_RaisesPropertyChanging_BeforeChanged()
    {
        var vm = new ProbeViewModel();
        var order = new List<string>();
        vm.PropertyChanging += (_, e) => order.Add($"changing:{e.PropertyName}");
        vm.PropertyChanged += (_, e) => order.Add($"changed:{e.PropertyName}");
        vm.Name = "n";
        Assert.Equal(["changing:Name", "changed:Name"], order);
    }

    [Fact]
    public void NotifyDependsOn_RaisesDependent()
    {
        var vm = new DependentViewModel();
        var names = new List<string>();
        vm.PropertyChanged += (_, e) => names.Add(e.PropertyName ?? "");
        vm.First = "Ada";
        Assert.Contains(nameof(DependentViewModel.First), names);
        Assert.Contains(nameof(DependentViewModel.FullName), names);
        Assert.Equal("Ada", vm.FullName);
    }

    [Fact]
    public void SetProperty_Callbacks_RunOnChangeOnly()
    {
        var vm = new DependentViewModel { WithCallback = "a" };
        Assert.Equal(1, vm.ChangingCalls);
        vm.WithCallback = "a";
        Assert.Equal(1, vm.ChangedCalls);
        vm.WithCallback = "b";
        Assert.Equal(2, vm.ChangedCalls);
    }

    [Fact]
    public void Notify_NullOrEmpty_Throws()
    {
        var vm = new ProbeViewModel();
        Assert.ThrowsAny<ArgumentException>(() => vm.RaiseNotify(null));
        Assert.ThrowsAny<ArgumentException>(() => vm.RaiseNotify(""));
    }

    [Fact]
    public void NotifyDependsOn_EmptyDependents_DoesNotThrow()
    {
        var vm = new DependentViewModel();
        var names = new List<string>();
        vm.PropertyChanged += (_, e) => names.Add(e.PropertyName ?? "");
        vm.Last = "Lovelace";
        Assert.Contains(nameof(DependentViewModel.FullName), names);
    }

    [Fact]
    public void SetProperty_CustomComparer_TreatsEqualAsNoChange()
    {
        var vm = new ProbeViewModel { Name = "Ada" };
        var count = 0;
        vm.PropertyChanged += (_, _) => count++;
        Assert.False(vm.SetName("ada", StringComparer.OrdinalIgnoreCase));
        Assert.Equal(0, count);
        Assert.True(vm.SetName("Grace", StringComparer.OrdinalIgnoreCase));
        Assert.Equal(1, count);
        Assert.Equal("Grace", vm.Name);
    }

    [Fact]
    public void PropertyEventArgs_AreCachedByName()
    {
        var first = PropertyEventArgsCache.ForChanged("Name");
        var second = PropertyEventArgsCache.ForChanged("Name");
        Assert.Same(first, second);
        Assert.NotSame(first, PropertyEventArgsCache.ForChanged("Title"));
    }
}
