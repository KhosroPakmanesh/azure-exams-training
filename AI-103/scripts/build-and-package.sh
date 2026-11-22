#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
bash "$ROOT/C#/scripts/publish-all.sh" "$@"
python3 "$ROOT/scripts/package.py"
