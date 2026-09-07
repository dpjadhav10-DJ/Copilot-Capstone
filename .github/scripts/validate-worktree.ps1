$ErrorActionPreference = 'Stop'
$status = @(git status --short)
$generated = $status | Where-Object {
    $_.Length -gt 1 -and $_[0] -ne ' ' -and $_ -match '\b(bin|obj|TestResults|\.vs|node_modules)[\\/]'
}
if ($generated) {
    Write-Error 'Generated or local-environment files are staged for publication. Exclude them unless explicitly approved:'
    $generated | ForEach-Object { Write-Error $_ }
    exit 1
}
& git diff --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& (Join-Path $PSScriptRoot 'validate-secrets.ps1') -Paths @('.')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Output 'Worktree whitespace, generated-output, and secret checks passed.'
