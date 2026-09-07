param(
    [string[]]$Paths
)
$ErrorActionPreference = 'Stop'
if (-not $Paths) { $Paths = @('.') }
$patterns = @(
    '(?i)(password|passwd|pwd)\s*[:=]\s*[^\s;"'']+',
    '(?i)(api[_-]?key|access[_-]?token|client[_-]?secret)\s*[:=]\s*[^\s;"'']+',
    '-----BEGIN (RSA|EC|OPENSSH) PRIVATE KEY-----',
    '(?i)AccountKey\s*=\s*[^;\s]+'
)
$files = Get-ChildItem -Path $Paths -File -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|\.git|TestResults|\.vs)\\' }
$findings = foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue
    foreach ($pattern in $patterns) {
        if ($content -match $pattern) { "$($file.FullName): likely secret pattern '$pattern'" }
    }
}
if ($findings) {
    $findings | ForEach-Object { Write-Error $_ }
    exit 1
}
Write-Output 'No likely secret patterns found in checked files.'
