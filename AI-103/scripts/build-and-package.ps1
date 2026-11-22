param([switch]$NoRestore)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot '../C#/scripts/publish-all.ps1') -NoRestore:$NoRestore
& python (Join-Path $PSScriptRoot 'package.py')
if ($LASTEXITCODE -ne 0) { throw 'Packaging failed.' }
