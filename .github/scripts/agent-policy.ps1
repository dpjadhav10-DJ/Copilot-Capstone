$ErrorActionPreference = 'Stop'
$inputText = [Console]::In.ReadToEnd()
try { $payload = $inputText | ConvertFrom-Json } catch { $payload = [pscustomobject]@{} }
$toolName = [string]$payload.toolName
if (-not $toolName) { $toolName = [string]$payload.tool_name }
$toolInput = $payload.toolInput
if (-not $toolInput) { $toolInput = $payload.tool_input }
$command = [string]$toolInput.command
if (-not $command) { $command = [string]$toolInput }
$normalized = $command.ToLowerInvariant()

function Write-Decision([string]$decision, [string]$reason) {
    [pscustomobject]@{
        hookSpecificOutput = [pscustomobject]@{
            hookEventName = 'PreToolUse'
            permissionDecision = $decision
            permissionDecisionReason = $reason
        }
    } | ConvertTo-Json -Compress
}

$blockedPatterns = @(
    'git reset --hard',
    'git clean -fd',
    'git push --force',
    'git push -f',
    'git branch -d',
    'git branch -D',
    'remove-item -recurse -force',
    'format-volume',
    'drop database',
    'truncate table'
)
foreach ($pattern in $blockedPatterns) {
    if ($normalized.Contains($pattern)) {
        Write-Decision 'deny' "Blocked dangerous operation matching '$pattern'. Use an explicit, reviewed manual operation if this is genuinely required."
        exit 0
    }
}

$secretPatterns = @(
    '(?i)(password|passwd|pwd)\s*[:=]\s*[^\s;]+',
    '(?i)(api[_-]?key|access[_-]?token|client[_-]?secret)\s*[:=]\s*[^\s;]+',
    '-----begin (rsa|ec|openssh) private key-----'
)
foreach ($pattern in $secretPatterns) {
    if ($command -match $pattern) {
        Write-Decision 'deny' 'Blocked a command containing a likely credential or private key. Use environment variables or a local secret store; never place secrets in prompts, files, or output.'
        exit 0
    }
}

if (($normalized -match 'git\s+(commit|push)\b|gh\s+pr\s+(create|merge)\b') -or ($normalized -match 'git\s+add\b.*\b(bin|obj|testresults|\.vs|node_modules)\b')) {
    Write-Decision 'ask' 'Repository or publication state will change, or generated output is being staged. Confirm scope, evidence, and approval before continuing.'
    exit 0
}

[pscustomobject]@{ continue = $true } | ConvertTo-Json -Compress
