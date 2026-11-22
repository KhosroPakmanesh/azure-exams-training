param([switch]$NoRestore)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')
Push-Location -LiteralPath $root
try {
    $solution = 'AI-103.CSharp.slnx'
    if (-not $NoRestore) {
        & dotnet restore $solution
        if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
    }

    [xml]$projects = Get-Content -LiteralPath $solution -Raw
    foreach ($project in $projects.SelectNodes('/Solution//Project')) {
        $projectPath = $project.GetAttribute('Path')
        $name = [IO.Path]::GetFileNameWithoutExtension($projectPath)
        & dotnet publish $projectPath -c Release --no-restore -o "../artifacts/publish/$name"
        if ($LASTEXITCODE -ne 0) { throw "Publish failed for $projectPath." }
    }
    Write-Host 'Published the solution to AI-103/artifacts/publish.'
}
finally {
    Pop-Location
}
