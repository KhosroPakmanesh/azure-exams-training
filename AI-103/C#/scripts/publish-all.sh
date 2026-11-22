#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
SOLUTION=AI-103.CSharp.slnx
PROJECTS="$(python3 ../scripts/verify-source.py --list-projects)"
if [[ "${1:-}" != "--no-restore" ]]; then
  dotnet restore "$SOLUTION"
fi
while IFS= read -r project; do
  project="${project%$'\r'}"
  name="$(basename "$project" .csproj)"
  dotnet publish "$project" -c Release --no-restore -o "../artifacts/publish/$name"
done <<< "$PROJECTS"
printf 'Published the solution to AI-103/artifacts/publish.\n'
