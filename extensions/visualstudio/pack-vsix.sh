#!/usr/bin/env bash
# Build an installable Visual Studio VSIX on this machine (Wine compiles the .vsct).
set -euo pipefail

out_vsix="${1:-}"
root="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$root/../.." && pwd)"

# Git Bash pwd is /d/a/...; Windows CPython treats that as \\d\\a\\..., not D:\a\...
native_path() {
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -w "$1"
  else
    printf '%s' "$1"
  fi
}

version="$(python3 -c "
from pathlib import Path
import re
import sys
text = Path(sys.argv[1]).read_text(encoding='utf-8')
match = re.search(r'<Version>([^<]+)</Version>', text)
if match is None:
    raise SystemExit('Directory.Build.props has no Version')
print(match.group(1))
" "$(native_path "$repo/Directory.Build.props")")"
stage="$root/obj/vsix-stage"
pack="$root/obj/vsct-pack"

dotnet restore "$root/WinUIMvvmExpress.VisualStudio.csproj" --nologo
dotnet build "$root/WinUIMvvmExpress.VisualStudio.csproj" -c Release --nologo --no-restore \
  -p:CreateVsixContainer=false \
  -p:DeployExtension=false \
  -p:GeneratePkgDefFile=false

props="$root/obj/WinUIMvvmExpress.VisualStudio.csproj.nuget.g.props"
if [ ! -f "$props" ]; then
  echo "Restore did not write $props" >&2
  exit 1
fi
vsdk_pkg="$(python3 -c "
from pathlib import Path
import re
import sys
text = Path(sys.argv[1]).read_text(encoding='utf-8')
m = re.search(r'PkgMicrosoft_VSSDK_BuildTools[^>]*>([^<]+)<', text)
if not m:
    raise SystemExit('PkgMicrosoft_VSSDK_BuildTools not found')
print(m.group(1))
" "$(native_path "$props")")"
vsdk="$vsdk_pkg/tools/vssdk"
if [ ! -x "$vsdk/bin/VSCT.exe" ] && [ ! -f "$vsdk/bin/VSCT.exe" ]; then
  echo "VSCT.exe not found under $vsdk" >&2
  exit 1
fi

mkdir -p "$pack" "$stage"
cp "$vsdk/bin/VSCT.exe" "$vsdk/bin/VSCT.exe.config" "$vsdk/bin/VSCTCompress.dll" "$vsdk/bin/VSCTLibrary.dll" "$pack/"
cp "$vsdk/inc/stdidcmd.h" "$vsdk/inc/vsshlids.h" "$root/WinUIMvvmExpressPackage.vsct" "$pack/"
(
  cd "$pack"
  if [[ "${OS:-}" == "Windows_NT" ]] || uname | grep -qiE 'mingw|msys|cygwin'; then
    ./VSCT.exe -nologo WinUIMvvmExpressPackage.vsct WinUIMvvmExpressPackage.cto
  elif command -v wine >/dev/null 2>&1; then
    WINEDEBUG=-all wine VSCT.exe -nologo WinUIMvvmExpressPackage.vsct WinUIMvvmExpressPackage.cto
  else
    echo "wine is required on macOS to compile WinUIMvvmExpressPackage.vsct. Install wine, or pack on Windows CI." >&2
    exit 1
  fi
)

rm -rf "$stage"
mkdir -p "$stage"
cp "$root/bin/Release/net472/NuvyntraLabs.WinUIMVVMExpress.VisualStudio.dll" "$stage/"
cp "$repo/LICENSE" "$stage/"

cat > "$stage/NuvyntraLabs.WinUIMVVMExpress.VisualStudio.pkgdef" <<'EOF'
[$RootKey$\Packages\{c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44}]
@="WinUIMvvmExpressPackage"
"InprocServer32"="$WinDir$\SYSTEM32\MSCOREE.DLL"
"Class"="NuvyntraLabs.WinUIMVVMExpress.VisualStudio.WinUIMvvmExpressPackage"
"CodeBase"="$PackageFolder$\NuvyntraLabs.WinUIMVVMExpress.VisualStudio.dll"
"AllowsBackgroundLoad"=dword:00000001

[$RootKey$\InstalledProducts\WinUI MVVMExpress]
"Package"="{c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44}"
"UseInterface"=dword:00000000
"ProductDetails"="Create WinUI MVVMExpress apps and pages from Plugin.WinUI.MVVMExpress.Templates."
"PID"="__VERSION__"

[$RootKey$\AutoLoadPackages\{adfc4e64-0397-11d1-9f4e-00a0c911004f}]
"{c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44}"=dword:00000002

[$RootKey$\AutoLoadPackages\{f1536ef8-92ec-443c-9ed7-fdadf150da82}]
"{c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44}"=dword:00000002

[$RootKey$\Menus]
"{c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44}"=", Menus.ctmenu, 1"
EOF
python3 -c "
from pathlib import Path
import sys
path = Path(sys.argv[1])
path.write_text(path.read_text(encoding='utf-8').replace('__VERSION__', sys.argv[2]), encoding='utf-8')
" "$(native_path "$stage/NuvyntraLabs.WinUIMVVMExpress.VisualStudio.pkgdef")" "$version"

cat > "$stage/extension.vsixmanifest" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011">
  <Metadata>
    <Identity Id="nuvyntralabs.WinUIMVVMExpress.VisualStudio" Version="${version}" Language="en-US" Publisher="Nuvyntra Labs" />
    <DisplayName>WinUI MVVMExpress for Visual Studio</DisplayName>
    <Description>Install Plugin.WinUI.MVVMExpress.Templates and create WinUI MVVMExpress apps and pages via dotnet new.</Description>
    <MoreInfo>https://github.com/nuvyntralabs/Plugin.WinUI.MVVMExpress</MoreInfo>
    <License>LICENSE</License>
    <Tags>WinUI;MVVM;templates;dotnet new;mvvmexpress</Tags>
  </Metadata>
  <Installation>
    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[17.0,19.0)">
      <ProductArchitecture>amd64</ProductArchitecture>
    </InstallationTarget>
    <InstallationTarget Id="Microsoft.VisualStudio.Community" Version="[17.0,19.0)">
      <ProductArchitecture>arm64</ProductArchitecture>
    </InstallationTarget>
    <InstallationTarget Id="Microsoft.VisualStudio.Professional" Version="[17.0,19.0)">
      <ProductArchitecture>amd64</ProductArchitecture>
    </InstallationTarget>
    <InstallationTarget Id="Microsoft.VisualStudio.Professional" Version="[17.0,19.0)">
      <ProductArchitecture>arm64</ProductArchitecture>
    </InstallationTarget>
    <InstallationTarget Id="Microsoft.VisualStudio.Enterprise" Version="[17.0,19.0)">
      <ProductArchitecture>amd64</ProductArchitecture>
    </InstallationTarget>
    <InstallationTarget Id="Microsoft.VisualStudio.Enterprise" Version="[17.0,19.0)">
      <ProductArchitecture>arm64</ProductArchitecture>
    </InstallationTarget>
  </Installation>
  <Dependencies>
    <Dependency Id="Microsoft.Framework.NDP" DisplayName="Microsoft .NET Framework" Version="[4.7.2,)" />
  </Dependencies>
  <Prerequisites>
    <Prerequisite Id="Microsoft.VisualStudio.Component.CoreEditor" Version="[17.0,19.0)" DisplayName="Visual Studio core editor" />
  </Prerequisites>
  <Assets>
    <Asset Type="Microsoft.VisualStudio.VsPackage" Path="NuvyntraLabs.WinUIMVVMExpress.VisualStudio.pkgdef" />
  </Assets>
</PackageManifest>
EOF

cp "$pack/WinUIMvvmExpressPackage.cto" "$stage/"

if [ -z "$out_vsix" ]; then
  out_vsix="$root/../dist/nuvyntralabs.WinUIMVVMExpress.VisualStudio.${version}.vsix"
fi
mkdir -p "$(dirname "$out_vsix")"
rm -f "$out_vsix"

export VSIX_STAGE="$(native_path "$stage")"
export VSIX_OUT="$(native_path "$out_vsix")"
python3 - <<PY
import hashlib
import json
import os
import zipfile
from pathlib import Path
from zipfile import ZipInfo

stage = Path(os.environ["VSIX_STAGE"])
out = Path(os.environ["VSIX_OUT"])
version = "$version"
identity = "nuvyntralabs.WinUIMVVMExpress.VisualStudio"
description = (
    "Install Plugin.WinUI.MVVMExpress.Templates and create WinUI MVVMExpress "
    "apps and pages via dotnet new."
)
payload = [
    "extension.vsixmanifest",
    "LICENSE",
    "WinUIMvvmExpressPackage.cto",
    "NuvyntraLabs.WinUIMVVMExpress.VisualStudio.dll",
    "NuvyntraLabs.WinUIMVVMExpress.VisualStudio.pkgdef",
]
extension_dir = r"[installdir]\Common7\IDE\Extensions\nuvyntralabs\WinUIMVVMExpress"

def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

files = [{"fileName": f"/{name}", "sha256": sha256(stage / name)} for name in payload]
install_size = sum((stage / name).stat().st_size for name in payload)
manifest = {
    "id": identity,
    "version": version,
    "type": "Vsix",
    "vsixId": identity,
    "extensionDir": extension_dir,
    "files": files,
    "installSizes": {"targetDrive": install_size},
    "dependencies": {
        "Microsoft.VisualStudio.Component.CoreEditor": "[17.0,19.0)"
    },
}
catalog = {
    "manifestVersion": "1.1",
    "info": {
        "id": f"{identity},version={version}",
        "manifestType": "Extension",
    },
    "packages": [
        {
            "id": f"Component.{identity}",
            "version": version,
            "type": "Component",
            "extension": True,
            "automaticallyAddedByExtensionPack": False,
            "dependencies": {
                identity: version,
                "Microsoft.VisualStudio.Component.CoreEditor": "[17.0,19.0)",
            },
            "localizedResources": [
                {
                    "language": "en-US",
                    "title": "WinUI MVVMExpress for Visual Studio",
                    "description": description,
                }
            ],
        },
        {
            "id": identity,
            "version": version,
            "type": "Vsix",
            "payloads": [{"fileName": out.name, "size": install_size}],
            "vsixId": identity,
            "extensionDir": extension_dir,
            "installSizes": {"targetDrive": install_size},
        },
    ],
}
manifest_bytes = json.dumps(manifest, separators=(",", ":")).encode("utf-8")
catalog_bytes = json.dumps(catalog, separators=(",", ":")).encode("utf-8")
(stage / "manifest.json").write_bytes(manifest_bytes)
(stage / "catalog.json").write_bytes(catalog_bytes)

# Match VSSDK Content_Types, plus types for the payload files it would add.
content_types = (
    "\ufeff<?xml version=\"1.0\" encoding=\"utf-8\"?>"
    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
    "<Default Extension=\"vsixmanifest\" ContentType=\"text/xml\" />"
    "<Default Extension=\"json\" ContentType=\"application/json\" />"
    "<Default Extension=\"dll\" ContentType=\"application/octet-stream\" />"
    "<Default Extension=\"pkgdef\" ContentType=\"text/plain\" />"
    "<Default Extension=\"cto\" ContentType=\"application/octet-stream\" />"
    "<Override PartName=\"/LICENSE\" ContentType=\"text/plain\" />"
    "</Types>"
).encode("utf-8")

def add(zf: zipfile.ZipFile, arcname: str, data: bytes) -> None:
    info = ZipInfo(filename=arcname)
    info.compress_type = zipfile.ZIP_DEFLATED
    info.create_system = 0
    zf.writestr(info, data)

with zipfile.ZipFile(out, "w", compression=zipfile.ZIP_DEFLATED, allowZip64=False) as zf:
    add(zf, "extension.vsixmanifest", (stage / "extension.vsixmanifest").read_bytes())
    add(zf, "manifest.json", manifest_bytes)
    add(zf, "catalog.json", catalog_bytes)
    add(zf, "[Content_Types].xml", content_types)
    for name in payload:
        if name == "extension.vsixmanifest":
            continue
        add(zf, name, (stage / name).read_bytes())
print(out)
PY
