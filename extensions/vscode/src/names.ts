export function isValidTypeName(name: string): boolean {
  return /^[A-Za-z_][A-Za-z0-9_]*$/.test(name);
}

export function detectNamespace(projectText: string, projectPath: string): string {
  const match = /<RootNamespace>\s*([^<]+)\s*<\/RootNamespace>/.exec(projectText);
  if (match) {
    const value = match[1].trim();
    if (value.length > 0) {
      return value;
    }
  }

  const fileName = projectPath.replace(/\\/g, "/").split("/").pop() ?? "";
  const stem = fileName.replace(/\.csproj$/i, "");
  return stem.length > 0 ? stem : "App1";
}

export function preferProjectPath(paths: string[]): string | undefined {
  const core = paths.find((path) => path.replace(/\\/g, "/").endsWith(".Core.csproj"));
  return core ?? paths[0];
}
