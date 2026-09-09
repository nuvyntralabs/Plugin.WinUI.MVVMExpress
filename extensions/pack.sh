#!/usr/bin/env bash
# Pack installable VS Code and Visual Studio extensions into extensions/dist/.
# Does not publish to a marketplace.
set -euo pipefail

root="$(cd "$(dirname "$0")" && pwd)"
dist="$root/dist"
version="$(python3 -c "
from pathlib import Path
import re
text = Path('$root/../Directory.Build.props').read_text()
match = re.search(r'<Version>([^<]+)</Version>', text)
if match is None:
    raise SystemExit('Directory.Build.props has no Version')
print(match.group(1))
")"
mkdir -p "$dist"

echo "Packing VS Code extension $version"
"$root/vscode/pack.sh" "$dist/nuvyntralabs.winui-mvvmexpress-${version}.vsix"

echo "Packing Visual Studio VSIX $version"
"$root/visualstudio/pack-vsix.sh" "$dist/nuvyntralabs.WinUIMVVMExpress.VisualStudio.${version}.vsix"

echo "Installable packages:"
ls -la "$dist"/*.vsix
