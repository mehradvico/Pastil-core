<#
.SYNOPSIS
    Whitelists the caller's current public IP for SQL Server (port 1433) on
    the Pastil server - and only that IP, replacing whatever this same tool
    whitelisted last time.

.DESCRIPTION
    1. Detects the caller's current public IPv4 (https://api.ipify.org).
    2. SSHes into the server and updates the DOCKER-USER iptables chain,
       which is what actually gates SQL Server's Docker-published port 1433
       on this box - not ufw (confirmed 2026-09-04: ufw isn't even
       installed; only its inert leftover chains remain in iptables. Docker
       inserts its own NAT/forward rules ahead of ufw's normal INPUT
       filtering, so DOCKER-USER is the chain that actually matters for a
       published port):
         - removes every existing "DROP everything except <ip>" rule for
           port 1433 in that chain, whatever IP it currently names,
         - inserts a fresh one for the caller's current IP.
    This keeps exactly one IP allowed through to SQL Server at a time - the
    caller's current one - rather than accumulating stale allowed IPs
    forever as networks change.

    Connection settings and credential storage are identical to
    Deploy-Backend.ps1 (scripts/.env.deploy, DPAPI-encrypted credential
    file) - this script only runs one remote command, no build/upload.

.PARAMETER SaveCredential
    Prompt for the SSH password and store it DPAPI-encrypted for future runs.

.PARAMETER ClearCredential
    Delete any stored credential and exit.

.EXAMPLE
    .\scripts\Whitelist-DatabaseIp.ps1

.EXAMPLE
    .\scripts\Whitelist-DatabaseIp.ps1 -SaveCredential
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
$RuleTag    = 'pastil-db-whitelist'

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
    <#
        Runs a native executable, streaming output live, returning the exit
        code. plink writes ordinary progress to stderr too, which would
        otherwise terminate the script under $ErrorActionPreference = 'Stop'
        (see Invoke-NativeCapture's comment) - relaxed to 'Continue' here for
        the same reason. Used (not Invoke-NativeCapture) for the multi-line
        remote script below - matches Deploy-Panel.ps1/Deploy-Website.ps1,
        which use this same function for their own multi-line remote steps.
    #>
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
Write-Host '  Pastil database IP whitelist' -ForegroundColor White
Write-Host ("  target: {0}@{1}" -f $ServerUser, $ServerHost) -ForegroundColor DarkGray

Write-Step 'Checking prerequisites'
$plinkExe = Resolve-Executable -Name 'plink' -Fallbacks @('C:\Program Files\PuTTY\plink.exe')
if (-not $plinkExe) { Stop-WithError 'plink not found. Install PuTTY (https://www.putty.org).' }
Write-Ok 'plink available'

# ---------------------------------------------------------------------------
# Current public IP
# ---------------------------------------------------------------------------

Write-Step 'Detecting current public IP'
try {
    $myIp = (Invoke-RestMethod -Uri 'https://api.ipify.org' -TimeoutSec 10).Trim()
} catch {
    Stop-WithError "Could not reach https://api.ipify.org to detect your IP: $($_.Exception.Message)"
}
if ($myIp -notmatch '^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$') {
    Stop-WithError "Got something that isn't an IPv4 address back: '$myIp'"
}
Write-Ok "Current public IP: $myIp"

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

$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-dbwhitelist-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
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
    # Connectivity check (also surfaces an uncached host key early)
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
    # Remote: drop old tagged rule(s), add the new one
    # -----------------------------------------------------------------------

    Write-Step "Updating DOCKER-USER (removing old port-1433 DROP rules, allowing only $myIp)"

    # Port 1433 is published by Docker (docker-proxy), and Docker's own
    # NAT/forward rules run before the regular INPUT chain ufw filters -
    # ufw itself turned out not to even be installed on this box (confirmed
    # 2026-09-04: `which ufw` empty, /usr/sbin/ufw missing, yet its old
    # ufw-* chains are still sitting inertly in iptables). Access control for
    # this port is actually a single DOCKER-USER rule of the form
    # "! -s <ip>/32 --dport 1433 -j DROP" (drop everything except that one
    # source IP) - confirmed against the live rule that was allowing
    # 2.190.91.12. Every existing DROP rule for port 1433 in that chain gets
    # removed (whatever IP(s) it names, comment-tagged by this tool or not -
    # the point is exactly one IP stays allowed) before inserting a fresh one
    # for the caller's current IP.
    $remoteScript = @"
set -e
IPT=/usr/sbin/iptables
`$IPT -S DOCKER-USER | grep -- '--dport 1433' | grep -- '-j DROP' | sed 's/^-A/-D/' | while read -r delrule; do
  `$IPT `$delrule
done
`$IPT -I DOCKER-USER 1 ! -s $myIp/32 -p tcp -m tcp --dport 1433 -m comment --comment '$RuleTag' -j DROP
echo '--> DOCKER-USER rules for port 1433 after update:'
`$IPT -S DOCKER-USER | grep 1433
"@

    $code = Invoke-NativeStreaming -Exe $plinkExe -Arguments @(
        '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost, $remoteScript)

    if ($code -ne 0) { Stop-WithError "Remote iptables update failed (exit code $code)" }
    Write-Ok "$myIp is now whitelisted for port 1433 (any previous IP from this tool was removed)"

    $elapsed = (Get-Date) - $script:StartTime
    Write-Host ''
    Write-Host ("  Done in {0:mm\:ss}." -f $elapsed) -ForegroundColor Green
    Write-Host ''

} finally {
    Remove-PasswordFile
}
