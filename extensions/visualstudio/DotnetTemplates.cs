using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NuvyntraLabs.WinUIMVVMExpress.VisualStudio;

internal sealed class DotnetResult
{
    public DotnetResult(int exitCode, string stdOut, string stdErr)
    {
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
    }

    public int ExitCode { get; }

    public string StdOut { get; }

    public string StdErr { get; }

    public string Combined => string.Join(Environment.NewLine, new[] { StdErr, StdOut }.Where(static part => !string.IsNullOrWhiteSpace(part)));
}

internal static class DotnetTemplates
{
    public const string PackageId = "Plugin.WinUI.MVVMExpress.Templates";
    public const string PackageVersion = "1.0.1";
    public const string AppShortName = "winui-mvvmexpress";
    public const string PageShortName = "winui-mvvmexpress-page";
    public const string DefaultAppName = "App1";
    public const string DefaultPageName = "Feature";
    public const string DefaultNamespace = "App1";

    public static async Task EnsureInstalledAsync(CancellationToken cancellationToken)
    {
        var probe = await RunAsync(new[] { "--version" }, workingDirectory: null, cancellationToken).ConfigureAwait(false);
        if (probe.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "Install the .NET SDK and ensure `dotnet` is on PATH." + Environment.NewLine + probe.Combined);
        }

        var listed = await RunAsync(new[] { "new", "list", AppShortName }, workingDirectory: null, cancellationToken)
            .ConfigureAwait(false);
        if (IsTemplateListed(listed.StdOut + Environment.NewLine + listed.StdErr, AppShortName))
        {
            return;
        }

        var install = await RunAsync(
                new[] { "new", "install", $"{PackageId}::{PackageVersion}" },
                workingDirectory: null,
                cancellationToken)
            .ConfigureAwait(false);
        if (install.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Could not install {PackageId} {PackageVersion}." + Environment.NewLine + install.Combined);
        }
    }

    public static async Task CreateAppAsync(string name, string outputDir, CancellationToken cancellationToken)
    {
        var result = await RunAsync(
                new[] { "new", AppShortName, "-n", name, "-o", outputDir, "--force" },
                workingDirectory: null,
                cancellationToken)
            .ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("Could not create the WinUI MVVMExpress app." + Environment.NewLine + result.Combined);
        }
    }

    public static async Task AddPageAsync(string name, string namespaceName, string outputDir, CancellationToken cancellationToken)
    {
        var result = await RunAsync(
                new[] { "new", PageShortName, "-n", name, "--namespace", namespaceName, "-o", outputDir, "--force" },
                outputDir,
                cancellationToken)
            .ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("Could not add the WinUI MVVMExpress page." + Environment.NewLine + result.Combined);
        }
    }

    public static bool IsValidTypeName(string name) => Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_]*$");

    public static bool IsValidNamespace(string name) => Regex.IsMatch(name, "^[A-Za-z_][A-Za-z0-9_.]*$");

    public static string DetectNamespace(string projectText, string projectPath)
    {
        var match = Regex.Match(projectText, @"<RootNamespace>\s*([^<]+)\s*</RootNamespace>");
        if (match.Success)
        {
            var value = match.Groups[1].Value.Trim();
            if (value.Length > 0)
            {
                return value;
            }
        }

        var stem = Path.GetFileNameWithoutExtension(projectPath);
        return string.IsNullOrWhiteSpace(stem) ? DefaultNamespace : stem;
    }

    public static string? PreferProjectPath(IReadOnlyList<string> paths)
    {
        foreach (var path in paths)
        {
            if (path.EndsWith(".Core.csproj", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }

        return paths.Count > 0 ? paths[0] : null;
    }

    internal static bool IsTemplateListed(string output, string shortName)
    {
        if (Regex.IsMatch(output, "No templates found matching", RegexOptions.IgnoreCase))
        {
            return false;
        }

        return Regex.IsMatch(output.Replace("\r", string.Empty), $@"(?:^|\s){Regex.Escape(shortName)}(?:\s|$)", RegexOptions.Multiline);
    }

    private static async Task<DotnetResult> RunAsync(IReadOnlyList<string> arguments, string? workingDirectory, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo
        {
            FileName = ResolveDotnet(),
            Arguments = string.Join(" ", arguments.Select(QuoteArgument)),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            start.WorkingDirectory = workingDirectory;
        }

        Process process;
        try
        {
            process = Process.Start(start) ?? throw new InvalidOperationException("Install the .NET SDK and ensure `dotnet` is on PATH.");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new InvalidOperationException("Install the .NET SDK and ensure `dotnet` is on PATH.", ex);
        }

        using (process)
        using (cancellationToken.Register(() => TryKill(process)))
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);
            if (!process.HasExited)
            {
                process.WaitForExit();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new DotnetResult(process.ExitCode, stdout, stderr);
        }
    }

    private static string ResolveDotnet()
    {
        var root = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(root))
        {
            var candidate = Path.Combine(root, "dotnet.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var installed = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");
        return File.Exists(installed) ? installed : "dotnet";
    }

    private static string QuoteArgument(string value)
    {
        if (value.IndexOfAny([' ', '"', '\t']) < 0)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch (InvalidOperationException)
        {
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
    }
}
