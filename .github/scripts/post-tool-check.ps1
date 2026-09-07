$ErrorActionPreference = 'Stop'
$inputText = [Console]::In.ReadToEnd()
try { $payload = $inputText | ConvertFrom-Json } catch { $payload = [pscustomobject]@{} }
$toolInput = $payload.toolInput
if (-not $toolInput) { $toolInput = $payload.tool_input }
$command = [string]$toolInput.command
if (-not $command) { $command = [string]$toolInput }

if ($command -match '(?i)\b(git\s+(add|commit|push)|gh\s+pr\s+(create|merge))\b') {
    & (Join-Path $PSScriptRoot 'validate-worktree.ps1')
    if ($LASTEXITCODE -ne 0) {
        [pscustomobject]@{ decision = 'block'; reason = 'Worktree validation failed after a repository mutation command.' } | ConvertTo-Json -Compress
        exit 0
    }
}

[pscustomobject]@{ continue = $true } | ConvertTo-Json -Compress
