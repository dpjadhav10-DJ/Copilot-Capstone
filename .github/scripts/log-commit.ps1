$ErrorActionPreference = 'Stop'
try {
    $gitDirectory = (git rev-parse --git-dir).Trim()
    $logDirectory = Join-Path $gitDirectory 'cafe-management-hook-logs'
    New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    $entry = [ordered]@{
        timestamp = (Get-Date).ToUniversalTime().ToString('o')
        branch = (git branch --show-current).Trim()
        commit = (git rev-parse HEAD).Trim()
        subject = (git log -1 --pretty=%s).Trim()
        author = (git log -1 --pretty='%an <%ae>').Trim()
        changedFiles = [int](git diff-tree --no-commit-id --name-only -r HEAD | Measure-Object -Line).Lines
    } | ConvertTo-Json -Compress
    Add-Content -LiteralPath (Join-Path $logDirectory 'commits.log') -Value $entry -Encoding UTF8
} catch {
    Write-Warning "Commit logging failed: $($_.Exception.Message)"
}
exit 0