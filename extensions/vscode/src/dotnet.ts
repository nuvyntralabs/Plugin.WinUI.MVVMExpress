import { spawn } from "child_process";
import { existsSync } from "fs";
import { join } from "path";
import { appShortName, pageShortName, templatePackageId, templatePackageVersion } from "./constants";

export class DotnetError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "DotnetError";
  }
}

export interface DotnetResult {
  code: number;
  stdout: string;
  stderr: string;
}

export function resolveDotnet(): string {
  const root = process.env.DOTNET_ROOT;
  if (root) {
    const executable = process.platform === "win32" ? "dotnet.exe" : "dotnet";
    const candidate = join(root, executable);
    if (existsSync(candidate)) {
      return candidate;
    }
  }

  return "dotnet";
}

export function runDotnet(args: string[], cwd?: string): Promise<DotnetResult> {
  return new Promise((resolve, reject) => {
    const child = spawn(resolveDotnet(), args, {
      cwd,
      env: process.env,
      windowsHide: true,
    });

    let stdout = "";
    let stderr = "";
    child.stdout.on("data", (chunk: Buffer | string) => {
      stdout += chunk.toString();
    });
    child.stderr.on("data", (chunk: Buffer | string) => {
      stderr += chunk.toString();
    });
    child.on("error", (error: NodeJS.ErrnoException) => {
      if (error.code === "ENOENT") {
        reject(new DotnetError("Install the .NET SDK and ensure `dotnet` is on PATH."));
        return;
      }

      reject(new DotnetError(error.message));
    });
    child.on("close", (code) => {
      resolve({ code: code ?? 1, stdout, stderr });
    });
  });
}

export function isTemplateListed(output: string, shortName: string): boolean {
  const combined = output.replace(/\r/g, "");
  if (/No templates found matching/i.test(combined)) {
    return false;
  }

  const token = new RegExp(`(?:^|\\s)${escapeRegExp(shortName)}(?:\\s|$)`, "m");
  return token.test(combined);
}

export async function ensureTemplatesInstalled(): Promise<void> {
  const probe = await runDotnet(["--version"]);
  if (probe.code !== 0) {
    throw new DotnetError(
      `Install the .NET SDK and ensure \`dotnet\` is on PATH.\n${formatOutput(probe)}`,
    );
  }

  const listed = await runDotnet(["new", "list", appShortName]);
  if (isTemplateListed(`${listed.stdout}\n${listed.stderr}`, appShortName)) {
    return;
  }

  const install = await runDotnet([
    "new",
    "install",
    `${templatePackageId}::${templatePackageVersion}`,
  ]);
  if (install.code !== 0) {
    throw new DotnetError(
      `Could not install ${templatePackageId} ${templatePackageVersion}.\n${formatOutput(install)}`,
    );
  }
}

export async function createApp(name: string, outputDir: string): Promise<void> {
  const result = await runDotnet(["new", appShortName, "-n", name, "-o", outputDir, "--force"]);
  if (result.code !== 0) {
    throw new DotnetError(`Could not create the WinUI MVVMExpress app.\n${formatOutput(result)}`);
  }
}

export async function addPage(name: string, namespaceName: string, outputDir: string): Promise<void> {
  const result = await runDotnet(
    ["new", pageShortName, "-n", name, "--namespace", namespaceName, "-o", outputDir, "--force"],
    outputDir,
  );
  if (result.code !== 0) {
    throw new DotnetError(`Could not add the WinUI MVVMExpress page.\n${formatOutput(result)}`);
  }
}

function formatOutput(result: DotnetResult): string {
  return [result.stderr, result.stdout].map((part) => part.trim()).filter((part) => part.length > 0).join("\n");
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
