namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>Marks a method for a generated <see cref="ModelCommand"/> named <c>{Method}Command</c>.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ModelCommandAttribute : Attribute
{
    /// <summary>Optional <c>CanExecute</c> method name (parameterless <see cref="bool"/>).</summary>
    public string? CanExecute { get; set; }
}

/// <summary>Marks a method for a generated <see cref="AsyncModelCommand"/> named <c>{Method}Command</c> (strips an <c>Async</c> suffix).</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AsyncModelCommandAttribute : Attribute
{
    /// <summary>Optional <c>CanExecute</c> method name (parameterless <see cref="bool"/>).</summary>
    public string? CanExecute { get; set; }
}
