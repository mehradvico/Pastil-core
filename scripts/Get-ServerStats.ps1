<#
.SYNOPSIS
    One-shot live-ish server stats snapshot (CPU load, memory, disk, per-
    container docker stats, uptime, recent API log lines) - polled every
    few seconds by the deploy app's monitoring screen.

.DESCRIPTION
    Same connection settings / DPAPI-encrypted credential as the other
    scripts/.env.deploy-based scripts. Never prompts (meant to be polled
    silently) - prints ERROR=... instead of blocking if no credential is
    stored yet.

    Output format: simple KEY=VALUE lines, then a literal "===LOGS==="
    marker line, then raw recent log text (not KEY=VALUE - shown as-is).
#>

[CmdletBinding()]
param(
    [string]$ServerHost,
    [string]$ServerUser
)

$ErrorActionPreference = 'Stop'

$ScriptsDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptsDir '.env.deploy'
$CredPath   = Join-Path $ScriptsDir '.deploy-credential.xml'

function Resolve-Executable {
    param([string]$Name, [string[]]$Fallbacks)
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    foreach ($candidate in $Fallbacks) {
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }
    return $null
}

$config = @{}
if (Test-Path -LiteralPath $ConfigPath) {
    foreach ($line in (Get-Content -LiteralPath $ConfigPath)) {
        $trimmed = $line.Trim()
        if (-not $trimmed -or $trimmed.StartsWith('#')) { continue }
        $separator = $trimmed.IndexOf('=')
        if ($separator -le 0) { continue }
        $key = $trimmed.Substring(0, $separator).Trim()
        $value = $trimmed.Substring($separator + 1).Trim().Trim('"').Trim("'")
        $config[$key] = $value
    }
}

if (-not $ServerHost) { $ServerHost = $config['PASTIL_DEPLOY_HOST'] }
if (-not $ServerUser) { $ServerUser = $config['PASTIL_DEPLOY_USER'] }
if (-not $ServerUser) { $ServerUser = 'root' }

if (-not $ServerHost) {
    Write-Output 'ERROR=No server host configured'
    exit 0
}

$plinkExe = Resolve-Executable -Name 'plink' -Fallbacks @('C:\Program Files\PuTTY\plink.exe')
if (-not $plinkExe) {
    Write-Output 'ERROR=plink not found'
    exit 0
}

$securePassword = $null
if (Test-Path -LiteralPath $CredPath) {
    try {
        $securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString -ErrorAction Stop
    } catch {
        $securePassword = $null
    }
}

if (-not $securePassword) {
    Write-Output 'ERROR=No stored credential yet - run a database action once with a saved password first'
    exit 0
}

$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-serverstats-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

# هر آمار با یه awk ساده و بدون نقل‌قول تودرتو گرفته می‌شود - همون درسی که از باگ
# قبلی (awk با رشته‌های تودرتو داخل plink) گرفتیم.
$remoteScript = @'
CORES=$(nproc)
LOAD1=$(awk '{print $1}' /proc/loadavg)
echo "CPU_CORES=$CORES"
echo "LOAD1=$LOAD1"

MEM_LINE=$(free -m | awk 'NR==2')
echo "MEM_TOTAL=$(echo "$MEM_LINE" | awk '{print $2}')"
echo "MEM_USED=$(echo "$MEM_LINE" | awk '{print $3}')"

DF_LINE=$(df -h / | tail -1)
echo "DISK_USED=$(echo "$DF_LINE" | awk '{print $3}')"
echo "DISK_TOTAL=$(echo "$DF_LINE" | awk '{print $2}')"
echo "DISK_PERCENT=$(echo "$DF_LINE" | awk '{print $5}')"

echo "UPTIME_SECONDS=$(awk '{print $1}' /proc/uptime)"

echo "===CONTAINERS==="
docker stats --no-stream --format '{{.Name}}::{{.CPUPerc}}::{{.MemUsage}}::{{.MemPerc}}' 2>/dev/null

echo "===LOGS==="
docker logs pastil-api-container --tail 30 2>&1
'@

try {
    $output = & $plinkExe -batch -ssh -l $ServerUser -pwfile $pwFile $ServerHost $remoteScript 2>&1
    $output | ForEach-Object { Write-Output $_ }
} catch {
    Write-Output "ERROR=$($_.Exception.Message)"
} finally {
    if (Test-Path -LiteralPath $pwFile) {
        try { [System.IO.File]::WriteAllText($pwFile, ('0' * 256)) } catch { }
        Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue
    }
}
