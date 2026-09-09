using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Plugin.WinUI.MVVMExpress.Templates.Tests;

public sealed class TemplateSmokeTests
{
    [Fact]
    public void Template_ContainsExpectedFiles()
    {
        var root = FindRepoRoot();
        var template = Path.Combine(root, "templates", "winui-app");
        Assert.True(File.Exists(Path.Combine(template, ".template.config", "template.json")));
        Assert.True(File.Exists(Path.Combine(template, "App1.sln")));
        Assert.True(File.Exists(Path.Combine(template, "App1", "App.xaml.cs")));
        Assert.True(File.Exists(Path.Combine(template, "App1.Core", "HomeViewModel.cs")));
        Assert.True(File.Exists(Path.Combine(template, "App1", "Pages", "HomePage.xaml")));
        Assert.True(File.Exists(Path.Combine(template, "App1.Core", "LoginViewModel.cs")));
        Assert.True(File.Exists(Path.Combine(root, "templates", "page", ".template.config", "template.json")));
        Assert.True(File.Exists(Path.Combine(template, "App1.Tests", "AppViewModelTests.cs")));
        Assert.True(File.Exists(Path.Combine(template, "Directory.Packages.props")));
    }

    [Fact]
    public void Instantiated_CoreAndTests_Pass()
    {
        var root = FindRepoRoot();
        var source = Path.Combine(root, "templates", "winui-app");
        var output = Path.Combine(root, "artifacts", "template-smoke", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        try
        {
            CopyTemplate(source, output);
            RewriteToProjectReferences(output, root);
            File.Copy(Path.Combine(root, "nuget.config"), Path.Combine(output, "nuget.config"), overwrite: true);
            File.WriteAllText(
                Path.Combine(output, "Directory.Build.props"),
                """
                <Project>
                  <PropertyGroup>
                    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
                  </PropertyGroup>
                </Project>
                """);
            File.WriteAllText(
                Path.Combine(output, "Directory.Packages.props"),
                """
                <Project>
                  <PropertyGroup>
                    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
                  </PropertyGroup>
                </Project>
                """);
            RunDotNet(["test", Path.Combine(output, "App1.Tests", "App1.Tests.csproj"), "-c", "Release", "--nologo"], output);
        }
        finally
        {
            TryDelete(output);
        }
    }

    private static void CopyTemplate(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part => part is "bin" or "obj" or ".template.config"))
            {
                continue;
            }

            var target = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    private static void RewriteToProjectReferences(string output, string repoRoot)
    {
        var core = Path.Combine(output, "App1.Core", "App1.Core.csproj");
        var tests = Path.Combine(output, "App1.Tests", "App1.Tests.csproj");
        File.WriteAllText(core, ReplacePackages(File.ReadAllText(core), Path.GetDirectoryName(core)!, repoRoot));
        File.WriteAllText(tests, ReplacePackages(File.ReadAllText(tests), Path.GetDirectoryName(tests)!, repoRoot));
    }

    private static string ReplacePackages(string csproj, string projectDir, string repoRoot)
    {
        string Ref(string project) => Path.GetRelativePath(projectDir, Path.Combine(repoRoot, "src", project, $"{project}.csproj"));

        csproj = Regex.Replace(
            csproj,
            """\s*<PackageReference Include="Plugin\.WinUI\.MVVMExpress\.Core" Version="\$\(WinUIMvvmExpressVersion\)" />""",
            $"""{Environment.NewLine}    <ProjectReference Include="{Ref("Plugin.WinUI.MVVMExpress.Core")}" />""");
        csproj = Regex.Replace(
            csproj,
            """\s*<PackageReference Include="Plugin\.WinUI\.MVVMExpress\.Pagination" Version="\$\(WinUIMvvmExpressVersion\)" />""",
            $"""{Environment.NewLine}    <ProjectReference Include="{Ref("Plugin.WinUI.MVVMExpress.Pagination")}" />""");
        csproj = Regex.Replace(
            csproj,
            """\s*<PackageReference Include="Plugin\.WinUI\.MVVMExpress\.Testing" Version="\$\(WinUIMvvmExpressVersion\)" />""",
            $"""{Environment.NewLine}    <ProjectReference Include="{Ref("Plugin.WinUI.MVVMExpress.Testing")}" />""");
        return csproj;
    }

    private static void RunDotNet(string[] args, string workingDirectory)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet failed to start.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet {string.Join(' ', args)} failed ({process.ExitCode}).{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "templates", "winui-app", ".template.config", "template.json")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not find the Plugin.WinUI.MVVMExpress repository root.");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
