#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
"$ROOT/scripts/publish-all.sh"
cd "$ROOT"
mkdir -p artifacts
ZIP_PATH="artifacts/FoundryCSharpLabs-With-Binaries.zip"
rm -f "$ZIP_PATH"
python3 - "$ROOT" "$ZIP_PATH" <<'PY'
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import sys
root=Path(sys.argv[1]); target=Path(sys.argv[2]);
include=['FoundryCSharpLabs.slnx','README.md','CHECKLIST.md','ASSEMBLIES.txt','Directory.Build.props','Directory.Packages.props','global.json','UPGRADE_NOTES.md','src','scripts','.github','artifacts/publish']
with ZipFile(target,'w',ZIP_DEFLATED) as z:
    for item in include:
        src=root/item
        files=src.rglob('*') if src.is_dir() else [src]
        for p in files:
            if p.is_file() and not any(part in {'obj','bin','.vs'} for part in p.relative_to(root).parts) and not (p.name == '.env' or (p.name.startswith('.env.') and p.name != '.env.example')):
                z.write(p,p.relative_to(root))
print('Created (no .env secrets included):',target)
PY
