using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Composition;

/// <summary>Attaches child ViewModels that receive parent lifecycle and dispose.</summary>
public interface IViewModelComposer
{
    /// <summary>Attached children.</summary>
    IReadOnlyList<IViewModel> Children { get; }

    /// <summary>Tracks <paramref name="child"/> until this instance is disposed.</summary>
    TChild Attach<TChild>(TChild child)
        where TChild : class, IViewModel;
}
