$ErrorActionPreference = 'Stop'
$staged = @(git diff --cached --name-only --diff-filter=ACMR)
if (-not $staged) { Write-Output 'No staged files.'; exit 0 }
$generated = $staged | Where-Object { $_ -match '(^|[\\/])(bin|obj|TestResults|\.vs|node_modules)([\\/]|$)' }
if ($generated) {
    $generated | ForEach-Object { Write-Error "Generated output must not be committed: $_" }
    exit 1
}
git diff --cached --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& (Join-Path $PSScriptRoot 'validate-secrets.ps1') -Paths $staged
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Output 'Staged-file checks passed.'
