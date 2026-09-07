<#
.SYNOPSIS
    Quick, read-only check of the last database backup on the Pastil server -
    prints simple KEY=VALUE lines (parsed by the deploy app's database
    screen to show a live status panel next to the backup button).

.DESCRIPTION
    Reads /root/pastil_app/backups/last-backup.info plus a couple of fresh
    numbers (current transaction log file size, disk usage) via a single
    plink call. Uses the same connection settings and DPAPI-encrypted
    credential as Backup-Database.ps1 / Whitelist-DatabaseIp.ps1 - never
    prompts if a credential is already stored (SendPushAsync-style silent
    fetch); prints an ERROR= line instead of exiting nonzero-noisily so the
    caller can show "unknown" rather than crashing the panel.

.PARAMETER SaveCredential
    Prompt for the SSH password and store it DPAPI-encrypted for future runs.
#>

[CmdletBinding()]
param(
    [string]$ServerHost,
    [string]$ServerUser,
    [switch]$SaveCredential
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
if ($SaveCredential) {
    $securePassword = Read-Host -Prompt "  SSH password for $ServerUser@$ServerHost" -AsSecureString
    $securePassword | ConvertFrom-SecureString | Set-Content -LiteralPath $CredPath -Encoding utf8
} elseif (Test-Path -LiteralPath $CredPath) {
    try {
        $securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString -ErrorAction Stop
    } catch {
        $securePassword = $null
    }
}

if (-not $securePassword) {
    # No stored credential and not asked to save one now - this call is meant
    # to be silent (called automatically when the screen opens), so it must
    # never block on Read-Host. Report clearly instead.
    Write-Output 'ERROR=No stored credential yet - use "Whitelist IP" or "بک‌آپ حالا" once with a saved password first'
    exit 0
}

$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-dbstatus-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

$remoteScript = @'
if [ -f /root/pastil_app/backups/last-backup.info ]; then
  cat /root/pastil_app/backups/last-backup.info
else
  echo "STATUS=none"
fi
echo "LOG_FILE_SIZE=$(docker exec pastil-sqlserver du -sh /var/opt/mssql/data/pastil_db_log.ldf 2>/dev/null | cut -f1)"
DF_LINE=$(df -h / | tail -1)
echo "DISK_USED=$(echo "$DF_LINE" | awk '{print $3}')"
echo "DISK_TOTAL=$(echo "$DF_LINE" | awk '{print $2}')"
echo "DISK_PERCENT=$(echo "$DF_LINE" | awk '{print $5}')"
echo "DISK_FREE=$(echo "$DF_LINE" | awk '{print $4}')"
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
