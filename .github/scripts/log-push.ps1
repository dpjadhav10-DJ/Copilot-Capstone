param(
    [string]$RemoteName,
    [string]$RemoteUrl
)
$ErrorActionPreference = 'Stop'
try {
    $gitDirectory = (git rev-parse --git-dir).Trim()
    $logDirectory = Join-Path $gitDirectory 'cafe-management-hook-logs'
    New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
    $safeRemoteUrl = if ($RemoteUrl) { $RemoteUrl -replace '(?i)(://|@)[^/@:]+:[^/@]+@', '$1***@' } else { '' }
    $lines = @([Console]::In.ReadToEnd() -split "`r?`n" | Where-Object { $_.Trim() })
    foreach ($line in $lines) {
        $parts = $line -split '\s+', 4
        if ($parts.Count -lt 4) { continue }
        $entry = [ordered]@{
            timestamp = (Get-Date).ToUniversalTime().ToString('o')
            remote = $RemoteName
            remoteUrl = $safeRemoteUrl
            localRef = $parts[0]
            localCommit = $parts[1]
            remoteRef = $parts[2]
            remoteCommit = $parts[3]
        } | ConvertTo-Json -Compress
        Add-Content -LiteralPath (Join-Path $logDirectory 'pushes.log') -Value $entry -Encoding UTF8
    }
} catch {
    Write-Warning "Push logging failed: $($_.Exception.Message)"
}
exit 0