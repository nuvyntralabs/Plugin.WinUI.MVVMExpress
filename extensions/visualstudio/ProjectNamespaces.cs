namespace NuvyntraLabs.WinUIMVVMExpress.VisualStudio;

internal static class ProjectNamespaces
{
    public static string Detect(string folder)
    {
        var nearby = FindCsprojNear(folder);
        var projects = nearby.Count > 0 ? nearby : FindCsproj(folder, depth: 4);
        var preferred = DotnetTemplates.PreferProjectPath(projects);
        if (preferred is null)
        {
            return DotnetTemplates.DefaultNamespace;
        }

        var text = File.ReadAllText(preferred);
        return DotnetTemplates.DetectNamespace(text, preferred);
    }

    private static List<string> FindCsprojNear(string folder)
    {
        var current = folder;
        for (var i = 0; i < 6; i++)
        {
            var matches = Directory.EnumerateFiles(current, "*.csproj", SearchOption.TopDirectoryOnly).ToList();
            if (matches.Count > 0)
            {
                return matches;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        return [];
    }

    private static List<string> FindCsproj(string root, int depth)
    {
        if (depth < 0 || !Directory.Exists(root))
        {
            return [];
        }

        var files = new List<string>();
        foreach (var path in Directory.EnumerateFileSystemEntries(root))
        {
            var name = Path.GetFileName(path);
            if (name is "bin" or "obj" or "node_modules" || name.StartsWith(".", StringComparison.Ordinal))
            {
                continue;
            }

            if (File.Exists(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(path);
            }
            else if (Directory.Exists(path))
            {
                files.AddRange(FindCsproj(path, depth - 1));
            }
        }

        return files;
    }
}
