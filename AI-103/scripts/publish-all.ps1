$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $root
& dotnet restore FoundryCSharpLabs.slnx
if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
& dotnet build FoundryCSharpLabs.slnx --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
$apps = '01.BasicChat','02.ResponsesChat','03.ContextChat','04.StreamingChat','05.AsyncChat','06.ChatHistory','07.ChatApi','08.BlazorChat'
foreach ($app in $apps) {
    & dotnet publish "src/$app/$app.csproj" --configuration Release --no-restore --output "artifacts/publish/$app"
    if ($LASTEXITCODE -ne 0) { throw "Publish failed: $app" }
}
Copy-Item 'src/Shared.FoundryClient/bin/Release/net11.0' 'artifacts/publish/Shared.FoundryClient' -Recurse -Force
Write-Host 'Done. Nine DLLs plus dependencies have been copied to artifacts/publish.'
