<#
.SYNOPSIS
    One-command deploy of the Pastil backend services (api / file / payment).

.DESCRIPTION
    For each requested service, automates the full manual pipeline:
        1. dotnet restore  <Service>/<Service>.csproj
        2. dotnet publish  -c Release -o publish-<service>
        3. docker build    -f <Service>/Dockerfile.runtime -t pastil-new-pastil-<service>:latest .
        4. docker save     -> pastil-<service>.tar
        5. scp             pastil-<service>.tar -> <remote>/<service>/
        6. ssh             docker load + docker compose up -d --no-deps --force-recreate <service>

    Shared work (prerequisites, SSH credential, connectivity, secret scan)
    runs once, then each service is built and deployed in turn.

    Connection settings live in scripts/.env.deploy (gitignored). The SSH
    password is prompted for each run unless -SaveCredential was used, in
    which case it is stored DPAPI-encrypted (per Windows user + machine) in
    scripts/.deploy-credential.xml, also gitignored.

.PARAMETER Service
    One or more of: api, file, payment. Defaults to api.

.PARAMETER All
    Deploy all three services (api, file, payment).

.PARAMETER SkipSecretScan
    Skip the Test-NoTrackedSecrets.ps1 pre-flight check. Not recommended.

.PARAMETER SaveCredential
    Prompt for the SSH password and store it DPAPI-encrypted for future runs.

.PARAMETER ClearCredential
    Delete any stored credential and exit.

.PARAMETER SkipBuild
    Reuse the existing .tar files instead of rebuilding. Useful to retry a
    failed upload without waiting for a full rebuild.

.EXAMPLE
    .\scripts\Deploy-Backend.ps1
    Deploys api only.

.EXAMPLE
    .\scripts\Deploy-Backend.ps1 -Service file

.EXAMPLE
    .\scripts\Deploy-Backend.ps1 -Service api,payment

.EXAMPLE
    .\scripts\Deploy-Backend.ps1 -All
#>

[CmdletBinding()]
param(
    # Not [ValidateSet]: when this script is launched via `powershell -File`,
    # every argument arrives as a raw string, so "-Service api,file" comes in
    # as the single value "api,file" rather than an array. Splitting and
    # validating by hand below makes both invocation styles work.
    [string[]]$Service = @('api'),

    [switch]$All,
    [string]$ServerHost,
    [string]$ServerUser,
    [string]$RemoteDir,
    [switch]$SkipSecretScan,
    [switch]$SkipBuild,
    [switch]$SaveCredential,
    [switch]$ClearCredential
)

$ErrorActionPreference = 'Stop'

$BackendRoot = Split-Path -Parent $PSScriptRoot
$ScriptsDir  = $PSScriptRoot
$ConfigPath  = Join-Path $ScriptsDir '.env.deploy'
$CredPath    = Join-Path $ScriptsDir '.deploy-credential.xml'

# Per-service build and deploy metadata. Keys match the docker compose
# service names in /root/pastil_app/docker-compose.yml.
$ServiceMap = [ordered]@{
    'api' = @{
        Project    = 'Api/Api.csproj'
        PublishDir = 'publish-api'
        Dockerfile = 'Api/Dockerfile.runtime'
        Image      = 'pastil-new-pastil-api:latest'
        Tar        = 'pastil-api.tar'
        RemoteSub  = 'api'
        Container  = 'pastil-api-container'
    }
    'file' = @{
        Project    = 'File/File.csproj'
        PublishDir = 'publish-file'
        Dockerfile = 'File/Dockerfile.runtime'
        Image      = 'pastil-new-pastil-file:latest'
        Tar        = 'pastil-file.tar'
        RemoteSub  = 'file'
        Container  = 'pastil-file-container'
    }
    'payment' = @{
        Project    = 'Payment/Payment.csproj'
        PublishDir = 'publish-payment'
        Dockerfile = 'Payment/Dockerfile.runtime'
        Image      = 'pastil-new-pastil-payment:latest'
        Tar        = 'pastil-payment.tar'
        RemoteSub  = 'payment'
        Container  = 'pastil-payment-container'
    }
}

if ($All) {
    $Service = @('api', 'file', 'payment')
}

# Accept "api,file" as one string (the `powershell -File` case) as well as a
# real array, and normalise case.
$requested = @(
    $Service |
        ForEach-Object { $_ -split ',' } |
        ForEach-Object { $_.Trim().ToLowerInvariant() } |
        Where-Object { $_ }
)

$unknown = @($requested | Where-Object { -not $ServiceMap.Contains($_) })
if ($unknown.Count -gt 0) {
    Write-Host ''
    Write-Host ("  Unknown service(s): {0}" -f ($unknown -join ', ')) -ForegroundColor Red
    Write-Host ("  Valid values are: {0}" -f (($ServiceMap.Keys) -join ', ')) -ForegroundColor Gray
    Write-Host ''
    exit 1
}

# Preserve canonical order (api, file, payment) and drop duplicates.
$targets = @($ServiceMap.Keys | Where-Object { $requested -contains $_ })

if ($targets.Count -eq 0) {
    Write-Host ''
    Write-Host '  No services selected.' -ForegroundColor Red
    Write-Host ''
    exit 1
}

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

function Write-Warn {
    param([string]$Message)
    Write-Host ("      !   {0}" -f $Message) -ForegroundColor Yellow
}

function Stop-WithError {
    param([string]$Message)
    Write-Host ''
    Write-Host ("  FAILED: {0}" -f $Message) -ForegroundColor Red
    Write-Host ''
    exit 1
}

function Invoke-NativeCapture {
    <#
        Runs a native executable and returns its combined output plus exit
        code. $ErrorActionPreference is deliberately set to SilentlyContinue
        inside this function: in Windows PowerShell 5.1, redirecting a native
        command's stderr wraps every line in a NativeCommandError, which
        would otherwise terminate under the script's global 'Stop' setting
        even when the command succeeded, or echo raw noise ahead of the
        caller's own message. Everything is captured in the returned Text,
        so the caller decides what to display.
    #>
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
        Runs a native executable, streaming its output live to the console,
        and returns the exit code.

        dotnet and docker both write ordinary progress to stderr. Under the
        script's global $ErrorActionPreference = 'Stop', Windows PowerShell
        5.1 turns that into a terminating NativeCommandError even when the
        command is succeeding, so this runs with 'Continue' in function
        scope and merges stderr into the display stream. Success is judged
        by the exit code alone, which is what the caller checks.
    #>
    param([string]$Exe, [string[]]$Arguments)

    $ErrorActionPreference = 'Continue'
    & $Exe @Arguments 2>&1 | ForEach-Object {
        # Merged stderr arrives as ErrorRecord objects whose ToString() is the
        # useless type name, so print the wrapped message instead.
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
if (-not $RemoteDir)  { $RemoteDir  = $config['PASTIL_DEPLOY_REMOTE_DIR'] }

if (-not $ServerUser) { $ServerUser = 'root' }
if (-not $RemoteDir)  { $RemoteDir  = '/root/pastil_app' }

if (-not $ServerHost) {
    Write-Host ''
    Write-Host '  No server host configured.' -ForegroundColor Yellow
    Write-Host ''
    Write-Host "  Create $ConfigPath with:" -ForegroundColor Gray
    Write-Host ''
    Write-Host '      PASTIL_DEPLOY_HOST=<server ip or hostname>' -ForegroundColor Gray
    Write-Host '      PASTIL_DEPLOY_USER=root' -ForegroundColor Gray
    Write-Host '      PASTIL_DEPLOY_REMOTE_DIR=/root/pastil_app' -ForegroundColor Gray
    Write-Host ''
    Write-Host '  (that file is gitignored), or pass -ServerHost <ip>.' -ForegroundColor Gray
    Write-Host ''
    exit 1
}

# ---------------------------------------------------------------------------
# Prerequisites
# ---------------------------------------------------------------------------

Write-Host ''
Write-Host '  Pastil backend deploy' -ForegroundColor White
Write-Host ("  services: {0}" -f ($targets -join ', ')) -ForegroundColor DarkGray
Write-Host ("  target:   {0}@{1}:{2}" -f $ServerUser, $ServerHost, $RemoteDir) -ForegroundColor DarkGray

Write-Step 'Checking prerequisites'

$dotnetExe = Resolve-Executable -Name 'dotnet' -Fallbacks @('C:\Program Files\dotnet\dotnet.exe')
$dockerExe = Resolve-Executable -Name 'docker' -Fallbacks @('C:\Program Files\Docker\Docker\resources\bin\docker.exe')
$pscpExe   = Resolve-Executable -Name 'pscp'   -Fallbacks @('C:\Program Files\PuTTY\pscp.exe')
$plinkExe  = Resolve-Executable -Name 'plink'  -Fallbacks @('C:\Program Files\PuTTY\plink.exe')

if (-not $dotnetExe) { Stop-WithError 'dotnet not found on PATH.' }
if (-not $dockerExe) { Stop-WithError 'docker not found on PATH.' }
if (-not $pscpExe)   { Stop-WithError 'pscp not found. Install PuTTY (https://www.putty.org).' }
if (-not $plinkExe)  { Stop-WithError 'plink not found. Install PuTTY (https://www.putty.org).' }

$dockerProbe = Invoke-NativeCapture -Exe $dockerExe -Arguments @('info', '--format', '{{.ServerVersion}}')
if ($dockerProbe.ExitCode -ne 0) {
    Stop-WithError 'Docker daemon is not responding. Is Docker Desktop running?'
}

foreach ($name in $targets) {
    $meta = $ServiceMap[$name]
    foreach ($required in @($meta.Project, $meta.Dockerfile)) {
        $path = Join-Path $BackendRoot $required
        if (-not (Test-Path -LiteralPath $path)) {
            Stop-WithError "Missing $required (needed for service '$name')."
        }
    }
}

Write-Ok 'dotnet, docker, pscp, plink and all service files present'

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
        Write-Warn 'Stored credential could not be decrypted (different user or machine). Prompting instead.'
        $securePassword = $null
    }
}

if (-not $securePassword) {
    $securePassword = Read-Host -Prompt "  SSH password for $ServerUser@$ServerHost" -AsSecureString
}

# Write the password to a temp file for plink/pscp -pwfile. This keeps it out
# of the process command line (where -pw would expose it to any process list).
$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-deploy-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    # No trailing newline: plink reads the whole first line as the password.
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

function Remove-PasswordFile {
    if (Test-Path -LiteralPath $pwFile) {
        try {
            # Overwrite before deleting so the plaintext does not linger on disk.
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
        $probeText = $probe.Text
        if ($probeText -match 'host key is not cached|server''s host key') {
            Write-Host ''
            Write-Host '  The server host key is not cached yet.' -ForegroundColor Yellow
            Write-Host '  Accept it once by running this and answering y:' -ForegroundColor Gray
            Write-Host ''
            Write-Host ("      plink -ssh {0}@{1}" -f $ServerUser, $ServerHost) -ForegroundColor Gray
            Write-Host ''
            Write-Host '  Then re-run this script.' -ForegroundColor Gray
            Stop-WithError 'Host key not cached.'
        }
        Write-Host $probeText -ForegroundColor DarkGray
        Stop-WithError 'SSH connection failed. Check host, user and password.'
    }

    Write-Ok "Connected to $ServerUser@$ServerHost"

    # -----------------------------------------------------------------------
    # Pre-flight: tracked secrets
    # -----------------------------------------------------------------------

    if ($SkipSecretScan) {
        Write-Step 'Secret scan (skipped)'
        Write-Warn '-SkipSecretScan was passed.'
    } else {
        Write-Step 'Scanning for tracked secrets'
        $scanScript = Join-Path $ScriptsDir 'Test-NoTrackedSecrets.ps1'
        if (Test-Path -LiteralPath $scanScript) {
            try {
                & $scanScript -RepositoryRoot $BackendRoot | Out-Null
                Write-Ok 'No tracked secrets found'
            } catch {
                Write-Host ''
                Write-Host ($_.Exception.Message) -ForegroundColor Red
                Stop-WithError 'Secret scan failed. Fix the findings, or re-run with -SkipSecretScan if they are false positives.'
            }
        } else {
            Write-Warn 'Test-NoTrackedSecrets.ps1 not found, skipping.'
        }
    }

    # -----------------------------------------------------------------------
    # Per-service build + deploy
    # -----------------------------------------------------------------------

    $deployed = @()

    foreach ($name in $targets) {
        $meta    = $ServiceMap[$name]
        $tarPath = Join-Path $BackendRoot $meta.Tar

        Write-Host ''
        Write-Host ("  --- {0} " -f $name.ToUpper()).PadRight(60, '-') -ForegroundColor Magenta

        if ($SkipBuild) {
            Write-Step "$name : build (skipped)"
            if (-not (Test-Path -LiteralPath $tarPath)) {
                Stop-WithError "-SkipBuild was passed but $tarPath does not exist."
            }
            Write-Warn ("Reusing existing {0}" -f $meta.Tar)
        } else {
            Push-Location $BackendRoot
            try {
                Write-Step "$name : dotnet restore"
                $code = Invoke-NativeStreaming -Exe $dotnetExe -Arguments @('restore', $meta.Project)
                if ($code -ne 0) { Stop-WithError "dotnet restore failed for $name (exit code $code)" }
                Write-Ok 'Restore complete'

                Write-Step "$name : dotnet publish (Release)"
                $code = Invoke-NativeStreaming -Exe $dotnetExe -Arguments @(
                    'publish', $meta.Project, '-c', 'Release', '-o', $meta.PublishDir,
                    '--no-restore', '/p:UseAppHost=false')
                if ($code -ne 0) { Stop-WithError "dotnet publish failed for $name (exit code $code)" }
                Write-Ok 'Publish complete'

                Write-Step "$name : docker build"
                $code = Invoke-NativeStreaming -Exe $dockerExe -Arguments @(
                    'build', '--no-cache', '-f', $meta.Dockerfile, '-t', $meta.Image, '.')
                if ($code -ne 0) { Stop-WithError "docker build failed for $name (exit code $code)" }
                Write-Ok ("Image built: {0}" -f $meta.Image)

                Write-Step "$name : docker save"
                if (Test-Path -LiteralPath $tarPath) {
                    Remove-Item -LiteralPath $tarPath -Force
                }
                $code = Invoke-NativeStreaming -Exe $dockerExe -Arguments @('save', '-o', $meta.Tar, $meta.Image)
                if ($code -ne 0) { Stop-WithError "docker save failed for $name (exit code $code)" }
                $tarSizeMb = [math]::Round((Get-Item -LiteralPath $tarPath).Length / 1MB, 1)
                Write-Ok ("{0} written ({1} MB)" -f $meta.Tar, $tarSizeMb)
            } finally {
                Pop-Location
            }
        }

        Write-Step "$name : uploading image"

        $tarSizeMb = [math]::Round((Get-Item -LiteralPath $tarPath).Length / 1MB, 1)
        Write-Host ("      transferring {0} MB, this is usually the slow part..." -f $tarSizeMb) -ForegroundColor DarkGray

        $remoteTarget = "{0}@{1}:{2}/{3}/{4}" -f $ServerUser, $ServerHost, $RemoteDir, $meta.RemoteSub, $meta.Tar
        $code = Invoke-NativeStreaming -Exe $pscpExe -Arguments @(
            '-batch', '-pwfile', $pwFile, $tarPath, $remoteTarget)
        if ($code -ne 0) { Stop-WithError "Upload failed for $name (exit code $code)" }
        Write-Ok 'Upload complete'

        Write-Step "$name : loading image and recreating container"

        $remoteCommand = @(
            "set -e",
            "cd '$RemoteDir'",
            ("docker load -i {0}/{1}" -f $meta.RemoteSub, $meta.Tar),
            ("docker compose up -d --no-deps --force-recreate {0}" -f $name)
        ) -join ' && '

        $code = Invoke-NativeStreaming -Exe $plinkExe -Arguments @(
            '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost, $remoteCommand)
        if ($code -ne 0) { Stop-WithError "Remote docker load / compose up failed for $name (exit code $code)" }
        Write-Ok 'Container recreated'

        Write-Step "$name : verifying"

        $status = Invoke-NativeCapture -Exe $plinkExe -Arguments @(
            '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost,
            ("docker ps --filter name={0} --format '{{{{.Status}}}}'" -f $meta.Container))
        $statusText = $status.Text.Trim()

        if ($statusText -match '^Up ') {
            Write-Ok ("{0} is {1}" -f $meta.Container, $statusText)
            $deployed += $name
        } else {
            Write-Warn ("Unexpected container status: '{0}'" -f $statusText)
            Write-Host ''
            Write-Host '      Recent logs:' -ForegroundColor DarkGray
            $logs = Invoke-NativeCapture -Exe $plinkExe -Arguments @(
                '-batch', '-ssh', '-l', $ServerUser, '-pwfile', $pwFile, $ServerHost,
                ('docker logs {0} --tail 40' -f $meta.Container))
            foreach ($line in ($logs.Text -split "`r?`n")) {
                Write-Host ("      {0}" -f $line) -ForegroundColor DarkGray
            }
            Stop-WithError "$name container is not running. See logs above."
        }
    }

    $elapsed = (Get-Date) - $script:StartTime
    Write-Host ''
    Write-Host ("  Deployed {0} in {1:mm\:ss}." -f ($deployed -join ', '), $elapsed) -ForegroundColor Green
    Write-Host ''
    Write-Host '  Useful follow-ups:' -ForegroundColor DarkGray
    foreach ($name in $deployed) {
        Write-Host ("      plink -ssh {0}@{1} ""docker logs {2} --tail 100""" -f $ServerUser, $ServerHost, $ServiceMap[$name].Container) -ForegroundColor DarkGray
    }
    Write-Host ''

} finally {
    Remove-PasswordFile
}
