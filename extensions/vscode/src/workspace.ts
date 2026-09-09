import { readdir, readFile } from "fs/promises";
import { dirname, join } from "path";
import { defaultNamespace } from "./constants";
import { detectNamespace, preferProjectPath } from "./names";

export async function detectWorkspaceNamespace(folder: string): Promise<string> {
  const nearby = await findCsprojNear(folder);
  const projects = nearby.length > 0 ? nearby : await findCsproj(folder, 4);
  const preferred = preferProjectPath(projects);
  if (!preferred) {
    return defaultNamespace;
  }

  const text = await readFile(preferred, "utf8");
  return detectNamespace(text, preferred);
}

async function findCsprojNear(folder: string): Promise<string[]> {
  let current = folder;
  for (let i = 0; i < 6; i++) {
    const matches = await listCsproj(current);
    if (matches.length > 0) {
      return matches;
    }

    const parent = dirname(current);
    if (parent === current) {
      break;
    }

    current = parent;
  }

  return [];
}

async function findCsproj(root: string, depth: number): Promise<string[]> {
  if (depth < 0) {
    return [];
  }

  const entries = await readdir(root, { withFileTypes: true }).catch(() => []);
  const files: string[] = [];
  for (const entry of entries) {
    if (entry.name === "bin" || entry.name === "obj" || entry.name === "node_modules" || entry.name.startsWith(".")) {
      continue;
    }

    const full = join(root, entry.name);
    if (entry.isFile() && entry.name.endsWith(".csproj")) {
      files.push(full);
    } else if (entry.isDirectory()) {
      files.push(...(await findCsproj(full, depth - 1)));
    }
  }

  return files;
}

async function listCsproj(folder: string): Promise<string[]> {
  const entries = await readdir(folder, { withFileTypes: true }).catch(() => []);
  return entries
    .filter((entry) => entry.isFile() && entry.name.endsWith(".csproj"))
    .map((entry) => join(folder, entry.name));
}
