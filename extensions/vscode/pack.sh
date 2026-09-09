#!/usr/bin/env bash
# Pack the VS Code / Cursor VSIX. Does not publish.
set -euo pipefail

out_vsix="${1:-}"
root="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$root/../.." && pwd)"
version="$(python3 -c "
from pathlib import Path
import re
text = Path('$repo/Directory.Build.props').read_text()
match = re.search(r'<Version>([^<]+)</Version>', text)
if match is None:
    raise SystemExit('Directory.Build.props has no Version')
print(match.group(1))
")"
if [ -z "$out_vsix" ]; then
  out_vsix="$root/../dist/nuvyntralabs.winui-mvvmexpress-${version}.vsix"
fi
mkdir -p "$(dirname "$out_vsix")"
if [ ! -f "$root/LICENSE" ]; then
  cp "$repo/LICENSE" "$root/LICENSE"
fi
(
  cd "$root"
  npm ci --ignore-scripts
  npm run compile
  npx --yes @vscode/vsce package --out "$out_vsix"
)
echo "$out_vsix"
