$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'validate-worktree.ps1')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& dotnet build CafeManagement.sln --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& dotnet test tests/CafeManagement.UiTests/CafeManagement.UiTests.csproj --no-build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Output 'Pre-push build and test validation passed.'
