$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $root
& (Join-Path $PSScriptRoot 'publish-all.ps1')
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Publishing failed' }
$zipPath = Join-Path $root 'artifacts/FoundryCSharpLabs-With-Binaries.zip'
$stage = Join-Path $root 'artifacts/package-staging'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Path $stage | Out-Null
$include = @('FoundryCSharpLabs.slnx','README.md','CHECKLIST.md','ASSEMBLIES.txt',
    'Directory.Build.props','Directory.Packages.props','global.json','UPGRADE_NOTES.md',
    'src','scripts','.github','artifacts/publish')
foreach ($path in $include) {
    $from = Join-Path $root $path
    $destination = Join-Path $stage $path
    $parent = Split-Path $destination -Parent
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    Copy-Item -LiteralPath $from -Destination $destination -Recurse -Force
}
# Never package filled .env files, even when building with compiled binaries.
Get-ChildItem -LiteralPath $stage -Recurse -Force -File | Where-Object {
    $_.Name -eq '.env' -or ($_.Name -like '.env.*' -and $_.Name -ne '.env.example')
} | Remove-Item -Force
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zipPath
Remove-Item $stage -Recurse -Force
Write-Host "Created $zipPath (credentials not included)"
