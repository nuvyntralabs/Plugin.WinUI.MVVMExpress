using System.Collections.Specialized;
using Plugin.WinUI.MVVMExpress.Collections;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Collections;

public sealed class ObservableRangeCollectionTests
{
    [Theory]
    [InlineData(ApplicationScale.Small)]
    [InlineData(ApplicationScale.Mid)]
    [InlineData(ApplicationScale.Large)]
    public void AddRange_RaisesSingleReset(ApplicationScale scale)
    {
        var n = ScaleProfile.ListSize(scale);
        var items = Enumerable.Range(0, n).ToList();
        var collection = new ObservableRangeCollection<int>();
        var events = 0;
        collection.CollectionChanged += (_, e) =>
        {
            events++;
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
        };

        collection.AddRange(items);

        Assert.Equal(n, collection.Count);
        Assert.Equal(1, events);
    }

    [Fact]
    public void Add_InLoop_RaisesPerItem_AndIsNotUsedForMidLarge()
    {
        var collection = new ObservableRangeCollection<int>();
        var events = 0;
        collection.CollectionChanged += (_, _) => events++;
        for (var i = 0; i < 10; i++)
        {
            collection.Add(i);
        }

        Assert.Equal(10, events);
    }

    [Fact]
    public void ReplaceRange_ReplacesContents_WithOneNotification()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };
        var events = 0;
        collection.CollectionChanged += (_, _) => events++;
        collection.ReplaceRange([9, 8, 7]);
        Assert.Equal(new[] { 9, 8, 7 }, collection);
        Assert.Equal(1, events);
    }

    [Fact]
    public void AddRange_Null_Throws()
        => Assert.Throws<ArgumentNullException>(() => new ObservableRangeCollection<int>().AddRange(null!));

    [Fact]
    public void AddRange_Empty_RaisesNothing()
    {
        var collection = new ObservableRangeCollection<int>();
        var events = 0;
        collection.CollectionChanged += (_, _) => events++;
        collection.AddRange([]);
        Assert.Equal(0, events);
    }

    [Fact]
    public void RemoveRange_RemovesMatching()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        var events = 0;
        collection.CollectionChanged += (_, _) => events++;
        collection.RemoveRange([2, 9]);
        Assert.Equal(new[] { 1, 3 }, collection);
        Assert.Equal(1, events);
    }

    [Fact]
    public void Reset_Clears()
    {
        var collection = new ObservableRangeCollection<int> { 1 };
        collection.Reset();
        Assert.Empty(collection);
        collection.Reset();
        Assert.Empty(collection);
    }

    [Fact]
    public void Constructor_CopiesItems()
    {
        var collection = new ObservableRangeCollection<int>([1, 2, 3]);
        Assert.Equal(new[] { 1, 2, 3 }, collection);
    }

    [Fact]
    public void RemoveRange_Null_Throws()
        => Assert.Throws<ArgumentNullException>(() => new ObservableRangeCollection<int>().RemoveRange(null!));

    [Fact]
    public void RemoveRange_NoMatch_RaisesNothing()
    {
        var collection = new ObservableRangeCollection<int> { 1 };
        var events = 0;
        collection.CollectionChanged += (_, _) => events++;
        collection.RemoveRange([9]);
        Assert.Equal(new[] { 1 }, collection);
        Assert.Equal(0, events);
    }

    [Fact]
    public void ReplaceRange_Null_Throws()
        => Assert.Throws<ArgumentNullException>(() => new ObservableRangeCollection<int>().ReplaceRange(null!));
}
