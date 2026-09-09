using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Navigation;

public sealed class NavigationRouteTableTests
{
    [Fact]
    public void Map_Resolve_RoundTripsPathAndIgnoresQuery()
    {
        var table = new NavigationRouteTable().Map<EmptyViewModel>("details");
        Assert.True(table.TryResolve("details?ProductId=2", out var type));
        Assert.Equal(typeof(EmptyViewModel), type);
        Assert.True(table.TryGetRoute(typeof(EmptyViewModel), out var route));
        Assert.Equal("details", route);
    }

    [Fact]
    public void Split_AndFormatQuery_RoundTripDictionary()
    {
        var (path, query) = NavigationRouteTable.Split("stack-item?Title=Latte&Depth=2");
        Assert.Equal("stack-item", path);
        Assert.Equal("Latte", query["Title"]);
        Assert.Equal("2", query["Depth"]);
        var formatted = NavigationRouteTable.FormatQuery(query);
        Assert.Contains("Title=Latte", formatted);
        Assert.Contains("Depth=2", formatted);
    }

    [Fact]
    public void FormatQuery_Object_SerializesPublicProperties()
    {
        var query = NavigationRouteTable.FormatQuery(new SampleArgs(7, "latte"));
        Assert.Contains("Id=7", query);
        Assert.Contains("Name=latte", query);
    }

    [Fact]
    public void MergeQuery_RightWins()
    {
        var left = new Dictionary<string, object> { ["A"] = "1", ["B"] = "2" };
        var right = new Dictionary<string, object> { ["B"] = "9" };
        var merged = NavigationRouteTable.MergeQuery(left, right);
        Assert.Equal("1", merged["A"]);
        Assert.Equal("9", merged["B"]);
    }

    [Fact]
    public void NavArgsApplier_AppliesTypedAndQuery()
    {
        var vm = new QueryViewModel();
        NavArgsApplier.ApplyTyped(vm, new SampleArgs(3, "tea"));
        NavArgsApplier.ApplyQuery(vm, new Dictionary<string, object> { ["Title"] = "tea" });
        Assert.Equal(3, vm.Id);
        Assert.Equal("tea", vm.Title);
    }

    private sealed class EmptyViewModel : ViewModel;

    private sealed record SampleArgs(int Id, string Name);

    private sealed class QueryViewModel : ViewModel, IAcceptNavArgs<SampleArgs>, IAcceptNavQuery
    {
        public int Id { get; private set; }

        public string? Title { get; private set; }

        public void Accept(SampleArgs args) => Id = args.Id;

        public void Accept(IReadOnlyDictionary<string, object> query)
            => Title = query.TryGetValue("Title", out var value) ? Convert.ToString(value) : null;
    }
}
