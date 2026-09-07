$ErrorActionPreference = 'Stop'
$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Push-Location $repositoryRoot
try {
    git config core.hooksPath .github/hooks
    if ($LASTEXITCODE -ne 0) { throw 'Unable to configure core.hooksPath.' }
    Write-Output 'Configured Git hooks to use .github/hooks.'
} finally {
    Pop-Location
}
