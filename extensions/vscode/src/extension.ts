import { join } from "path";
import * as vscode from "vscode";
import { defaultAppName, defaultPageName } from "./constants";
import { addPage, createApp, DotnetError, ensureTemplatesInstalled } from "./dotnet";
import { isValidTypeName } from "./names";
import { detectWorkspaceNamespace } from "./workspace";

export function activate(context: vscode.ExtensionContext): void {
  context.subscriptions.push(
    vscode.commands.registerCommand("winuimvvmexpress.createApp", () => runCreateApp()),
    vscode.commands.registerCommand("winuimvvmexpress.addPage", () => runAddPage()),
  );
}

export function deactivate(): void {
  // Commands are disposed with the extension context.
}

async function runCreateApp(): Promise<void> {
  try {
    await ensureTemplatesInstalled();
    const name = await askName("App name", defaultAppName);
    if (!name) {
      return;
    }

    const folder = await vscode.window.showOpenDialog({
      canSelectFiles: false,
      canSelectFolders: true,
      canSelectMany: false,
      openLabel: "Create app here",
      title: "WinUI MVVMExpress app folder",
    });
    const parent = folder?.[0]?.fsPath;
    if (!parent) {
      return;
    }

    const outputDir = join(parent, name);
    await vscode.window.withProgress(
      { location: vscode.ProgressLocation.Notification, title: `Creating ${name}…` },
      async () => createApp(name, outputDir),
    );

    const open = await vscode.window.showInformationMessage(
      `Created ${name} at ${outputDir}.`,
      "Open folder",
    );
    if (open === "Open folder") {
      await vscode.commands.executeCommand("vscode.openFolder", vscode.Uri.file(outputDir), true);
    }
  } catch (error) {
    showError(error);
  }
}

async function runAddPage(): Promise<void> {
  try {
    const workspace = vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
    if (!workspace) {
      throw new DotnetError("Open a folder or workspace before adding a page.");
    }

    await ensureTemplatesInstalled();
    const name = await askName("Page name (without Page suffix)", defaultPageName);
    if (!name) {
      return;
    }

    const suggested = await detectWorkspaceNamespace(workspace);
    const namespaceName = await vscode.window.showInputBox({
      title: "WinUI MVVMExpress",
      prompt: "Root namespace",
      value: suggested,
      ignoreFocusOut: true,
      validateInput: (value) =>
        /^[A-Za-z_][A-Za-z0-9_.]*$/.test(value.trim()) ? undefined : "Enter a C# namespace.",
    });
    if (!namespaceName) {
      return;
    }

    const folder = await vscode.window.showOpenDialog({
      canSelectFiles: false,
      canSelectFolders: true,
      canSelectMany: false,
      defaultUri: vscode.Uri.file(workspace),
      openLabel: "Add page here",
      title: "WinUI MVVMExpress page folder",
    });
    const outputDir = folder?.[0]?.fsPath;
    if (!outputDir) {
      return;
    }

    await vscode.window.withProgress(
      { location: vscode.ProgressLocation.Notification, title: `Adding ${name} page…` },
      async () => addPage(name, namespaceName.trim(), outputDir),
    );

    void vscode.window.showInformationMessage(
      `Created ${name} page in ${outputDir}. Call services.Add${name}() and map the route in App.xaml.cs.`,
    );
  } catch (error) {
    showError(error);
  }
}

async function askName(prompt: string, value: string): Promise<string | undefined> {
  const name = await vscode.window.showInputBox({
    title: "WinUI MVVMExpress",
    prompt,
    value,
    ignoreFocusOut: true,
    validateInput: (input) =>
      isValidTypeName(input.trim()) ? undefined : "Use a C# type name (letters, digits, underscore).",
  });
  return name?.trim();
}

function showError(error: unknown): void {
  const message = error instanceof Error ? error.message : String(error);
  void vscode.window.showErrorMessage(message);
}
