#!/usr/bin/env python3
"""Fail unless versions in the requested scope match Directory.Build.props."""

from __future__ import annotations

import argparse
import os
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def fail(message: str) -> None:
    print(f"::error::{message}")
    raise SystemExit(1)


def local_name(tag: str) -> str:
    return tag.split("}")[-1]


def first_property(path: Path, name: str) -> str | None:
    try:
        tree = ET.parse(path)
    except (ET.ParseError, OSError) as exc:
        fail(f"Could not parse {path}: {exc}")
    for element in tree.iter():
        if local_name(element.tag) != name:
            continue
        if element.attrib.get("Include") or element.attrib.get("include"):
            continue
        text = (element.text or "").strip()
        if text and not text.startswith("$("):
            return text
    return None


def write_output(name: str, value: str) -> None:
    output = os.environ.get("GITHUB_OUTPUT")
    if not output:
        return
    with open(output, "a", encoding="utf-8") as handle:
        handle.write(f"{name}={value}\n")


def packable(csproj: Path) -> bool:
    flag = first_property(csproj, "IsPackable")
    test = first_property(csproj, "IsTestProject")
    if test == "true":
        return False
    return flag != "false"


def project_version(csproj: Path, product: str) -> str:
    for name in ("PackageVersion", "Version"):
        value = first_property(csproj, name)
        if value:
            return value
    return product


def collect_library(plugin_root: Path, product: str) -> dict[str, str]:
    versions: dict[str, str] = {}
    src = plugin_root / "src"
    if not src.is_dir():
        fail(f"No src/ under {plugin_root}")
    packable_projects = [
        path
        for path in sorted(src.rglob("*.csproj"))
        if "bin" not in path.parts and "obj" not in path.parts and packable(path)
    ]
    if not packable_projects:
        fail("No packable src/*.csproj")
    templates = [path for path in packable_projects if path.stem.endswith(".Templates")]
    if not templates:
        fail("No packable Templates csproj under src/")

    for path in packable_projects:
        versions[str(path.relative_to(plugin_root))] = project_version(path, product)

    template_props = plugin_root / "templates/winui-app/Directory.Build.props"
    template_version = first_property(template_props, "WinUIMvvmExpressVersion")
    if not template_version:
        fail(f"{template_props} has no WinUIMvvmExpressVersion")
    versions[str(template_props.relative_to(plugin_root)) + " WinUIMvvmExpressVersion"] = template_version
    return versions


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--plugin-root", default=".")
    parser.add_argument("--scope", choices=("library", "all"), default="library")
    args = parser.parse_args()
    plugin_root = Path(args.plugin_root).resolve()
    props = plugin_root / "Directory.Build.props"
    product = first_property(props, "Version")
    if not product:
        fail(f"{props} has no Version")
    versions = {str(Path("Directory.Build.props")) + " Version": product}
    versions.update(collect_library(plugin_root, product))
    print(f"Version alignment (library, must all equal):")
    mismatched: list[str] = []
    for label, version in versions.items():
        mark = "OK" if version == product else "MISMATCH"
        print(f"  [{mark}] {version}  {label}")
        if version != product:
            mismatched.append(f"{label}={version}")
    if mismatched:
        fail(f"Versions must match {product}. Fix: " + ", ".join(mismatched))
    write_output("version", product)
    print(f"Version {product} is aligned (library)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
