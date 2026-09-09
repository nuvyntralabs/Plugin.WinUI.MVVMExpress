namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>Execution state of an async command.</summary>
public enum CommandExecutionState
{
    /// <summary>Not running.</summary>
    Idle = 0,

    /// <summary>Currently executing.</summary>
    Running = 1,

    /// <summary>Last run completed without error.</summary>
    Completed = 2,

    /// <summary>Last run failed.</summary>
    Failed = 3,

    /// <summary>Last run was cancelled.</summary>
    Cancelled = 4
}
