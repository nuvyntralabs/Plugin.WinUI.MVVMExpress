using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Collections;

/// <summary>
/// <see cref="ObservableCollection{T}"/> with batch updates so mid and large lists do not raise one event per item.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>Creates an empty collection.</summary>
    public ObservableRangeCollection()
    {
    }

    /// <summary>Creates a collection with the given items.</summary>
    public ObservableRangeCollection(IEnumerable<T> items)
        : base(items)
    {
    }

    /// <summary>Adds items and raises a single <see cref="NotifyCollectionChangedAction.Reset"/>.</summary>
    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var batch = items as ICollection<T> ?? items.ToList();
        if (batch.Count == 0)
        {
            return;
        }

        CheckReentrancy();
        foreach (var item in batch)
        {
            Items.Add(item);
        }

        RaiseReset();
    }

    /// <summary>Removes items that match <paramref name="items"/> and raises a single reset.</summary>
    public void RemoveRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var removed = false;
        CheckReentrancy();
        foreach (var item in items)
        {
            removed |= Items.Remove(item);
        }

        if (removed)
        {
            RaiseReset();
        }
    }

    /// <summary>
    /// Replaces the entire contents and raises a single reset.
    /// Unsafe on a visible Android <c>BindableLayout</c> after appear — seed before bind, or mutate with <c>Add</c>.
    /// </summary>
    public void ReplaceRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        CheckReentrancy();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }

        RaiseReset();
    }

    /// <summary>Clears the collection using a single reset notification.</summary>
    public void Reset()
    {
        if (Items.Count == 0)
        {
            return;
        }

        CheckReentrancy();
        Items.Clear();
        RaiseReset();
    }

    private void RaiseReset()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
