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
    Write-Output 'ERROR=No stored credential yet'
    exit 0
}

$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-webapplogs-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

$remoteScript = @'
echo "===STATUS==="
docker ps -a --filter "name=pastil-frontend-container" --format "{{.Names}}::{{.Status}}"
echo "===LOGS==="
docker logs pastil-frontend-container --tail 150 2>&1
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
