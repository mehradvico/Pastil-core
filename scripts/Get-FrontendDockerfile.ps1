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

$plinkExe = Resolve-Executable -Name 'plink' -Fallbacks @('C:\Program Files\PuTTY\plink.exe')

$securePassword = (Get-Content -LiteralPath $CredPath -Raw).Trim() | ConvertTo-SecureString
$pwFile = Join-Path ([System.IO.Path]::GetTempPath()) ("pastil-dockerfile-{0}.tmp" -f ([guid]::NewGuid().ToString('N')))
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [System.IO.File]::WriteAllText($pwFile, $plainPassword, (New-Object System.Text.UTF8Encoding($false)))
    $plainPassword = $null
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
}

$remoteScript = @'
cd /root/pastil_app
echo "===COMPOSE-FRONTEND-SECTION==="
awk '/^  frontend:/,/^  [a-zA-Z]/' docker-compose.yml | head -40
echo "===DOCKERFILE==="
find . -maxdepth 3 -iname "Dockerfile*" -path "*frontend*" 2>/dev/null
find frontend -maxdepth 2 -iname "Dockerfile*" 2>/dev/null
cat frontend/Dockerfile 2>/dev/null
echo "===OUTPUT-SERVER-NODEMODULES-CHECK==="
ls frontend/.output/server/node_modules 2>/dev/null | grep -i iconify
test -d frontend/.output/server/node_modules/@iconify && echo "ICONIFY_PRESENT_ON_DISK=yes" || echo "ICONIFY_PRESENT_ON_DISK=no"
'@

try {
    $output = & $plinkExe -batch -ssh -l $ServerUser -pwfile $pwFile $ServerHost $remoteScript 2>&1
    $output | ForEach-Object { Write-Output $_ }
} finally {
    if (Test-Path -LiteralPath $pwFile) {
        try { [System.IO.File]::WriteAllText($pwFile, ('0' * 256)) } catch { }
        Remove-Item -LiteralPath $pwFile -Force -ErrorAction SilentlyContinue
    }
}
