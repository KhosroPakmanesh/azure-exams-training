#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"
dotnet restore FoundryCSharpLabs.slnx
dotnet build FoundryCSharpLabs.slnx -c Release --no-restore
mkdir -p artifacts/publish
for p in 01.BasicChat 02.ResponsesChat 03.ContextChat 04.StreamingChat 05.AsyncChat 06.ChatHistory 07.CodeInterpreter 08.WebSearch 09.FileSearch 10.FunctionCalling 11.ToolsChat; do
  dotnet publish "src/$p/$p.csproj" -c Release --no-restore -o "artifacts/publish/$p"
done
cp -r src/Shared.FoundryClient/bin/Release/net11.0 artifacts/publish/Shared.FoundryClient
printf 'Done. Twelve DLLs plus dependencies have been copied to artifacts/publish.\n'
