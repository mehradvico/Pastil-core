<#
.SYNOPSIS
    Triggers a manual full database backup on the Pastil server (via the
    pastil-backup-now cron system already installed there) and pulls the
    resulting backup files down to D:\WorkSpace\Backups on this machine.

.DESCRIPTION
    1. SSHes into the server (plink) and runs
       /root/pastil_app/scripts/pastil-backup-now.sh, streaming its output
       live - this takes a fresh SQL Server backup (compression-free, since
       the server runs SQL Server Express) and prints the resulting status
       report (last backup time/size, transaction log size, disk usage).
    2. Copies the whole /root/pastil_app/backups directory (full .bak files
       + logs/ subfolder of .trn files) down via pscp into a timestamped
       folder under D:\WorkSpace\Backups.

    Connection settings and credential storage are identical to
    Deploy-Backend.ps1 / Whitelist-DatabaseIp.ps1 (scripts/.env.deploy,
    DPAPI-encrypted credential file) - this script only runs one remote
    command plus a file copy, no build/upload.

.PARAMETER SaveCredential
    Prompt for the SSH password and store it DPAPI-encrypted for future runs.

.PARAMETER ClearCredential
    Delete any stored credential and exit.

.EXAMPLE
    .\scripts\Backup-Database.ps1

.EXAMPLE
    .\scripts\Backup-Database.ps1 -SaveCredential
#>

[CmdletBinding()]
param(
    [string]$ServerHost,
    [string]$ServerUser,
    [switch]$SaveCredential,
    [switch]$ClearCredential
)

$ErrorActionPreference = 'Stop'

$ScriptsDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptsDir '.env.deploy'
$CredPath   = Join-Path $ScriptsDir '.deploy-credential.xml'
$RemoteBackupDir = '/root/pastil_app/backups'
$LocalBackupRoot = 'D:\WorkSpace\Backups'

$script:StepIndex = 0
$script:StartTime = Get-Date

function Write-Step {
    param([string]$Message)
    $script:StepIndex++
    Write-Host ''
    Write-Host ("  [{0}] {1}" -f $script:StepIndex, $Message) -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host ("      OK  {0}" -f $Message) -ForegroundColor Green
}

function Stop-WithError {
    param([string]$Message)
    Write-Host ''
    Write-Host ("  FAILED: {0}" -f $Message) -ForegroundColor Red
    Write-Host ''
    exit 1
}

function Invoke-NativeCapture {
    param([string]$Exe, [string[]]$Arguments)
    $ErrorActionPreference = 'SilentlyContinue'
    $output = & $Exe @Arguments 2>&1
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Text     = ($output | Out-String)
    }
}

function Invoke-NativeStreaming {
    param([string]$Exe, [string[]]$Arguments)
    $ErrorActionPreference = 'Continue'
    & $Exe @Arguments 2>&1 | ForEach-Object {
        if ($_ -is [System.Management.Automation.ErrorRecord]) {
            Write-Host $_.Exception.Message
        } else {
            Write-Host $_
        }
    }
    return $LASTEXITCODE
}

function Resolve-Executable {
    param([string]$Name, [string[]]$Fallbacks)
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    foreach ($candidate in $Fallbacks) {
        if (Test-Path -LiteralPath $candidate) { return $candidate }
    }
    return $null
}

# ---------------------------------------------------------------------------
# Credential housekeeping
# ---------------------------------------------------------------------------

if ($ClearCredential) {
    if (Test-Path -LiteralPath $CredPath) {
        Remove-Item -LiteralPath $CredPath -Force
        Write-Host 'Stored deploy credential removed.' -ForegroundColor Green
    } else {
        Write-Host 'No stored credential to remove.' -ForegroundColor Yellow
    }
    exit 0
}

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

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
    Write-Host ''
    Write-Host '  No server host configured.' -ForegroundColor Yellow
    Write-Host "  Create $ConfigPath with PASTIL_DEPLOY_HOST=<ip> (same file Deploy-Backend.ps1 uses)," -ForegroundColor Gray
    Write-Host '  or pass -ServerHost <ip>.' -ForegroundColor Gray
    Write-Host ''
    exit 1
}

Write-Host ''
Write-Host '  Pastil database backup' -ForegroundColor White
Write-Host ("  target: {0}@{1}" -f $ServerUser, $ServerHost) -ForegroundColor DarkGray

Write-Step 'Checking prerequisites'
$plinkExe = Resolve-Executable -Name 'plink' -Fallbacks @('C:\Program Files\PuTTY\plink.exe')
if (-not $plinkExe) { Stop-WithError 'plink not found. Install PuTTY (https://www.putty.org).' }
Write-Ok 'plink available'
$pscpExe = Resolve-Executable -Name 'pscp' -Fallbacks @('C:\Program Files\PuTTY\pscp.exe')
if (-not $pscpExe) { Stop-WithError 'pscp not found. Install PuTTY (https://www.putty.org).' }
Write-Ok 'pscp available'

# ---------------------------------------------------------------------------
# Credential
# ---------------------------------------------------------------------------

Write-Step 'Resolving SSH credential'

$securePassword = $null

if ($SaveCredential) {
    $securePassword = Read-Host -Prompt "  SSH password for $ServerUser@$ServerHost" -AsSecureString
    $securePassword | ConvertFrom-SecureString | Set-Content -LiteralPath $CredPath -Encoding utf8
    Write-Ok "Credential saved (DPAPI-encrypted) to $CredPath"
} elseif (Test-Path -LiteralPath $CredPath) {
    try {
        $securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString -ErrorAction Stop
        Write-Ok 'Using stored credential'
    } catch {
        Write-Host '      !   Stored credential could not be decrypted. Prompting instead.' -ForegroundColor Yellow
        $securePassword = $null
    }
}

if (-not $securePassword) {
    $securePassword = Read-Host -Prompt "  SSH password for $ServerUser@$ServerHost" -AsSecureString
}

$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-dbbackup-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

function Remove-PasswordFile {
    if (Test-Path -LiteralPath $pwFile) {
        try {
            [System.IO.File]::WriteAllText($pwFile, ('0' * 256))
        } catch { }
        Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue
    }
}

try {

    # -----------------------------------------------------------------------
    # Connectivity check
    # -----------------------------------------------------------------------

    Write-Step 'Testing SSH connection'

    $probe = Invoke-NativeCapture -Exe $plinkExe -Arguments @(
        '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost, 'echo pastil-ssh-ok')
    if ($probe.ExitCode -ne 0) {
        if ($probe.Text -match 'host key is not cached|server''s host key') {
            Write-Host ''
            Write-Host '  The server host key is not cached yet.' -ForegroundColor Yellow
            Write-Host '  Accept it once by running this and answering y:' -ForegroundColor Gray
            Write-Host ''
            Write-Host ("      plink -ssh {0}@{1}" -f $ServerUser, $ServerHost) -ForegroundColor Gray
            Write-Host ''
            Write-Host '  Then re-run this script.' -ForegroundColor Gray
            Stop-WithError 'Host key not cached.'
        }
        Write-Host $probe.Text -ForegroundColor DarkGray
        Stop-WithError 'SSH connection failed. Check host, user and password.'
    }
    Write-Ok "Connected to $ServerUser@$ServerHost"

    # -----------------------------------------------------------------------
    # Remote: run the manual backup + status report
    # -----------------------------------------------------------------------

    Write-Step 'Running manual backup on the server'

    $code = Invoke-NativeStreaming -Exe $plinkExe -Arguments @(
        '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost,
        '/root/pastil_app/scripts/pastil-backup-now.sh')

    if ($code -ne 0) { Stop-WithError "Remote backup failed (exit code $code)" }
    Write-Ok 'Server-side backup completed'

    # -----------------------------------------------------------------------
    # Pull backup files down locally
    # -----------------------------------------------------------------------

    Write-Step 'Copying backup files to this machine'

    $stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm'
    $localDir = Join-Path $LocalBackupRoot $stamp
    New-Item -ItemType Directory -Force -Path $localDir | Out-Null

    $copyCode = Invoke-NativeStreaming -Exe $pscpExe -Arguments @(
        '-batch', '-scp', '-r', '-pwfile', $pwFile,
        "$ServerUser@${ServerHost}:$RemoteBackupDir",
        $localDir)

    if ($copyCode -ne 0) { Stop-WithError "Copying backup files failed (exit code $copyCode)" }
    Write-Ok "Backup files saved to $localDir"

    $elapsed = (Get-Date) - $script:StartTime
    Write-Host ''
    Write-Host ("  Done in {0:mm\:ss}. Local copy: {1}" -f $elapsed, $localDir) -ForegroundColor Green
    Write-Host ''

} finally {
    Remove-PasswordFile
}
